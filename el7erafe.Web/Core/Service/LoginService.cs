using Azure.Core;
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Helpers;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace Service
{
    public class LoginService(UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IClientRepository clientRepository,
        ITechnicianRepository technicianRepository,
        OtpHelper otpHelper,
        ILogger<LoginService> logger,
        IUserTokenRepository userTokenRepository) : ILoginService
    {
        public async Task<UserDTO> LoginAsync(LoginDTO loginDTO)
        {
            logger.LogInformation("[SERVICE] Login attempt started for phone number: {PhoneNumber}",
                loginDTO.PhoneNumber);

            logger.LogInformation("[SERVICE] Looking up user by phone number: {PhoneNumber}",
                loginDTO.PhoneNumber);

            var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDTO.PhoneNumber);

            if (user is null)
            {
                logger.LogWarning("[SERVICE] Login failed: User not found for phone number: {PhoneNumber}",
                    loginDTO.PhoneNumber);
                throw new UnauthorizedUserException();
            }

            logger.LogInformation("[SERVICE] User found with ID: {UserId}, UserType: {UserType}, checking password",
                user.Id, user.UserType);

            var result = await userManager.CheckPasswordAsync(user, loginDTO.Password);

            if (!result)
            {
                logger.LogWarning("[SERVICE] Login failed: Invalid password for user: {UserId}", user.Id);
                throw new UnauthorizedUserException();
            }

            var userToken = await userTokenRepository.GetUserTokenAsync(user.Id);
            if (userToken is not null)
            {
                throw new Exception("المستخدم مسجل الدخول بالفعل.");
            }

            logger.LogInformation("[SERVICE] Password verification successful for user: {UserId}", user.Id);

            if (user.UserType == UserTypeEnum.Client)
            {
                logger.LogInformation("[SERVICE] Processing client login flow for user: {UserId}", user.Id);

                if (!user.EmailConfirmed)
                {
                    logger.LogWarning("[SERVICE] Client email not verified for user: {UserId}, sending OTP", user.Id);

                    await otpHelper.SendOTP(user);

                    logger.LogWarning("[SERVICE] Throwing UnverifiedClientLogin for user: {UserId}", user.Id);
                    throw new UnverifiedClientLogin(user.Email!);
                }

                logger.LogInformation("[SERVICE] Client email verified, proceeding with login for user: {UserId}", user.Id);

                logger.LogInformation("[SERVICE] Retrieving client details for user: {UserId}", user.Id);
                var client = await clientRepository.GetByUserIdAsync(user.Id);

                if (client is null)
                {
                    logger.LogError("[SERVICE] Client record not found for user: {UserId}", user.Id);
                    throw new UnauthorizedUserException();
                }

                logger.LogInformation("[SERVICE] Generating token for client: {ClientName}", client.Name);
                var token = await new CreateToken(userManager, configuration).CreateTokenAsync(user);

                await userTokenRepository.CreateUserTokenAsync(new UserToken
                {
                    Token = token,
                    Type = TokenType.Token,
                    UserId = user.Id
                });

                logger.LogInformation("[SERVICE] Client login completed successfully: {ClientName} ({UserId})",
                    client.Name, user.Id);

                return new UserDTO
                {
                    userId = client.UserId,
                    userName = client.Name,
                    type = 'C',
                    token = token
                };
            }
            else
            {
                logger.LogInformation("[SERVICE] Retrieving technician details for user: {UserId}", user.Id);
                var technician = await technicianRepository.GetFullTechnicianByUserIdAsync(user.Id);
                if (technician is null)
                {
                    logger.LogError("[SERVICE] Technician record not found for user: {UserId}", user.Id);
                    throw new UnauthorizedUserException();
                }

                logger.LogInformation("[SERVICE] Processing technician login flow for user: {UserId}", user.Id);
                if (technician.Status == TechnicianStatus.Pending)
                {
                    logger.LogWarning("[SERVICE] Technician login rejected - status Pending for user: {UserId}", user.Id);
                    var createToken = new CreateToken(userManager, configuration);
                    var accessToken = await createToken.CreateTokenAsync(user);

                    await userTokenRepository.CreateUserTokenAsync(new UserToken
                    {
                        Token = accessToken,
                        Type = TokenType.TempToken,
                        UserId = user.Id
                    });
                    throw new PendingTechnicianRequest(accessToken);
                }

                else if (technician.Status == TechnicianStatus.Rejected)
                {
                    logger.LogWarning("[SERVICE] Technician login rejected - status Rejected for user: {UserId}", user.Id);
                    throw new RejectedTechnician(technician);
                }
                else if (technician.Status == TechnicianStatus.Blocked)
                {
                    logger.LogWarning("[SERVICE] Technician login rejected - status Blocked for user: {UserId}", user.Id);
                    throw new BlockedTechnician();
                }

                logger.LogInformation("[SERVICE] Technician status approved, proceeding with login for user: {UserId}",
                        user.Id);

                logger.LogInformation("[SERVICE] Generating token for technician: {TechnicianName}", technician.Name);
                var token = await new CreateToken(userManager, configuration).CreateTokenAsync(user);

                logger.LogInformation("[SERVICE] Technician login completed successfully: {TechnicianName} ({UserId})",
                    technician.Name, user.Id);

                await userTokenRepository.CreateUserTokenAsync(new UserToken
                {
                    Token = token,
                    Type = TokenType.Token,
                    UserId = user.Id
                });

                return new UserDTO
                {
                    userId = technician.UserId,
                    userName = technician.Name,
                    type = 'T',
                    token = token
                };
            }
        }

        public async Task<OtpResponseDTO> ForgetPasswordAsync(ResendOtpRequestDTO forgetPasswordDTO)
        {
            logger.LogInformation("[SERVICE] Checking if email is registered: {Email}", forgetPasswordDTO.Email);
            var user = await userManager.FindByEmailAsync(forgetPasswordDTO.Email);
            if (user is null)
            {
                logger.LogWarning("[SERVICE] Email not registered: {Email}", forgetPasswordDTO.Email);
                throw new UserNotFoundException("البريد الإلكتروني غير مسجل");
            }

            if (!otpHelper.CanResendOtp(user.Id).Result)
            {
                logger.LogWarning("[SERVICE] OTP already sent recently for: {Email}", user.Email);
                throw new OtpAlreadySent();
            }

            if (!user.EmailConfirmed)
            {
                logger.LogWarning("[SERVICE] Client email not verified for user: {UserId}, sending OTP", user.Id);

                await otpHelper.SendOTP(user);

                logger.LogWarning("[SERVICE] Throwing ForgotPasswordDisallowed for user: {UserId}", user.Id);
                throw new ForgotPasswordDisallowed();
            }

            return new OtpResponseDTO
            {
                Message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني."
            };
        }

        public async Task<UserDTO> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO, string userId, TimeSpan timeDifference)
        {
            try
            {
                if (timeDifference.TotalMinutes > 5)
                {
                    await userTokenRepository.DeleteUserTokenAsync(userId);
                    throw new ResetTokenExpiredException();
                }

                var checkForTempToken = await userTokenRepository.GetUserTokenAsync(userId);
                if (checkForTempToken!.Type != TokenType.TempToken)
                {
                    throw new TechnicalException();
                }

                var user = await userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    throw new UserNotFoundException("المستخدم غير موجود");
                }

                var result = await userManager.CheckPasswordAsync(user, resetPasswordDTO.Password);
                if (result is true)
                {
                    throw new PasswordReuseException();
                }

                var removeResult = await userManager.RemovePasswordAsync(user);
                if (!removeResult.Succeeded)
                {
                    throw new BadRequestException(removeResult.Errors.Select(e => e.Description).ToList());
                }

                var addResult = await userManager.AddPasswordAsync(user, resetPasswordDTO.Password);
                if (!addResult.Succeeded)
                {
                    throw new BadRequestException(addResult.Errors.Select(e => e.Description).ToList());
                }

                await userTokenRepository.DeleteUserTokenAsync(userId);
                var token = await new CreateToken(userManager, configuration).CreateTokenAsync(user);
                await userTokenRepository.CreateUserTokenAsync(new UserToken
                {
                    Token = token,
                    Type = TokenType.Token,
                    UserId = user.Id
                });

                var client = await clientRepository.GetByUserIdAsync(userId);
                if (client is null)
                    throw new TechnicalException();

                return new UserDTO
                {
                    userId = user.Id,
                    userName = client!.Name,
                    type = 'C',
                    token = token
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}