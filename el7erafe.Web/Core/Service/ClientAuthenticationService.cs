using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace Service
{
    public class ClientAuthenticationService(UserManager<ApplicationUser> userManager,
        IClientRepository clientRepository,
        IConfiguration configuration,
        OtpHelper otpHelper,
        ILogger<ClientAuthenticationService> logger,
        IUserTokenRepository userTokenRepository) : IClientAuthenticationService
    {
        public async Task<OtpResponseDTO> RegisterAsync(ClientRegisterDTO clientRegisterDTO)
        {
            logger.LogInformation("[SERVICE] Checking phone number uniqueness: {Phone}", clientRegisterDTO.PhoneNumber);
            var phoneNumberFound = await clientRepository.ExistsAsync(clientRegisterDTO.PhoneNumber);

            if (phoneNumberFound)
            {
                logger.LogWarning("[SERVICE] Duplicate phone number detected: {Phone}", clientRegisterDTO.PhoneNumber);
                throw new PhoneNumberAlreadyExists(clientRegisterDTO.PhoneNumber);
            }

            logger.LogInformation("[SERVICE] Checking email uniqueness: {Email}", clientRegisterDTO.Email);
            var userFound = await clientRepository.EmailExistsAsync(clientRegisterDTO.Email);

            if (userFound)
            {
                logger.LogWarning("[SERVICE] Duplicate email detected: {Email}", clientRegisterDTO.Email);
                throw new EmailAlreadyExists(clientRegisterDTO.Email);
            }

            logger.LogInformation("[Service] Starting registration with OTP for: {Email}", clientRegisterDTO.Email);

            var user = new ApplicationUser
            {
                UserName = clientRegisterDTO.PhoneNumber,
                Email = clientRegisterDTO.Email,
                PhoneNumber = clientRegisterDTO.PhoneNumber,
                EmailConfirmed = false,
                UserType = UserTypeEnum.Client
            };

            var result = await userManager.CreateAsync(user, clientRegisterDTO.Password);

            if (!result.Succeeded)
            {
                logger.LogError("[SERVICE] User creation failed for email: {Email}", clientRegisterDTO.Email);
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new BadRequestException(errors);
            }

            var client = new Client
            {
                Name = clientRegisterDTO.Name,
                UserId = user.Id
            };

            await clientRepository.CreateAsync(client);
            await userManager.AddToRoleAsync(user, "Client");

            await otpHelper.SendOTP(user);

            logger.LogInformation("[Service] User created(unconfirmed) and OTP sent: {Email}"
            , clientRegisterDTO.Email);

            logger.LogInformation("[SERVICE] Client registration completed for: {Email}", clientRegisterDTO.Email);

            return new OtpResponseDTO
            {
                Message = "تم إرسال الرمز إلى بريدك الإلكتروني. يرجى التحقق لإكمال التسجيل حتى تتمكن من تسجيل الدخول بنجاح."
            };
        }

        public async Task<UserDTO> ConfirmEmailAsync(OtpVerificationDTO otpVerificationDTO)
        {
            logger.LogInformation("[Service] Completing registration with OTP for: {Email}", otpVerificationDTO.Email);

            var user = await userManager.FindByEmailAsync(otpVerificationDTO.Email);
            if (user is null)
                throw new UserNotFoundException("المستخدم غير موجود.");

            logger.LogInformation("[Service] Checking if email is already verified: {Email}", otpVerificationDTO.Email);
            if (user.EmailConfirmed)
            {
                logger.LogWarning("[Service] Email already verified: {Email}", otpVerificationDTO.Email);
                throw new EmailAlreadyVerified("الحساب مفعل بالفعل. يرجى تسجيل الدخول");
            }

            var identifier = otpHelper.GetOtpIdentifier(user.Id);
            var result = await otpHelper.VerifyOtp(identifier, otpVerificationDTO.OtpCode);

            if (!result)
            {
                throw new InvalidOtpException();
            }

            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
            await userManager.AddToRoleAsync(user, "Client");

            var client = await clientRepository.GetByUserIdAsync(user.Id);
            if (client is null)
                throw new UserNotFoundException("المستخدم غير موجود.");

            logger.LogInformation("[Service] Registration completed with OTP verification: {Email}", otpVerificationDTO.Email);

            var token = await new CreateToken(userManager, configuration).CreateTokenAsync(user);
            await userTokenRepository.CreateUserTokenAsync(new UserToken
            {
                Token = token,
                Type = TokenType.Token,
                UserId = user.Id
            });
            return new UserDTO
            {
                token = token,
                userId = client.UserId,
                userName = client.Name,
                type = 'C'
            };
        }

        public async Task<OtpResponseDTO> ResendOtp(ResendOtpRequestDTO resendOtpRequestDTO)
        {
            logger.LogInformation("[SERVICE] Checking if email is registered: {Email}", resendOtpRequestDTO.Email);
            var user = await userManager.FindByEmailAsync(resendOtpRequestDTO.Email);
            if (user is null)
            {
                logger.LogWarning("[SERVICE] Email not registered: {Email}", resendOtpRequestDTO.Email);
                throw new UserNotFoundException("البريد الإلكتروني غير مسجل");
            }

            var identifier = otpHelper.GetOtpIdentifier(user.Id);
            logger.LogInformation("[SERVICE] Checking if OTP was sent more than 60 seconds ago to: {Email}", resendOtpRequestDTO.Email);
            if (!otpHelper.CanResendOtp(identifier).Result)
            {
                logger.LogWarning("[SERVICE] OTP already sent recently for: {Email}", resendOtpRequestDTO.Email);
                throw new OtpAlreadySent();
            }

            await otpHelper.SendOTP(user);

            logger.LogInformation("[Service] Resend OTP sent to: {Email}"
            , resendOtpRequestDTO.Email);

            return new OtpResponseDTO
            {
                Message = "تم إرسال الرمز إلى بريدك الإلكتروني."
            };
        }

        public async Task VerifyOtpAsync(OtpVerificationDTO otpVerificationDTO)
        {
            logger.LogInformation("[SERVICE] Checking if email is registered: {Email}", otpVerificationDTO.Email);
            var user = await userManager.FindByEmailAsync(otpVerificationDTO.Email);
            if (user is null)
            {
                logger.LogWarning("[SERVICE] Email not registered: {Email}", otpVerificationDTO.Email);
                throw new UserNotFoundException("البريد الإلكتروني غير مسجل");
            }
            var identifier = otpHelper.GetOtpIdentifier(user.Id);
            var result = await otpHelper.VerifyOtp(identifier, otpVerificationDTO.OtpCode);

            if (!result)
            {
                throw new InvalidOtpException();
            }
        }

        public async Task<ResetOTP> VerifyResetOtpAsync(OtpVerificationDTO otpVerificationDTO)
        {
            await VerifyOtpAsync(otpVerificationDTO);
            logger.LogInformation("[SERVICE] Temp Token generated for reset password for user: {Email}", otpVerificationDTO.Email);
            var user = await userManager.FindByEmailAsync(otpVerificationDTO.Email);
            if (user is null)
            {
                logger.LogWarning("[SERVICE] Email not registered: {Email}", otpVerificationDTO.Email);
                throw new UserNotFoundException("البريد الإلكتروني غير مسجل");
            }

            await userTokenRepository.DeleteUserTokenAsync(user.Id);
            var token = await new CreateToken(userManager, configuration).CreateTokenAsync(user);
            await userTokenRepository.CreateUserTokenAsync(new UserToken
            {
                Token = token,
                Type = TokenType.TempToken,
                UserId = user.Id
            });
            return new ResetOTP
            {
                tempToken = token
            };
        }
    }
}
