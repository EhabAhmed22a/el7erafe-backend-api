using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OtpDTOs;
using static System.Net.WebRequestMethods;

namespace Service
{
    public class ClientAuthenticationService(UserManager<ApplicationUser> userManager,
        IClientRepository clientRepository,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<ClientAuthenticationService> logger) : IClientAuthenticationService
    {
        public async Task<OtpResponseDTO> RegisterAndSendOtpAsync(ClientRegisterDTO clientRegisterDTO)
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

            var identifier = GetOtpIdentifier(user.Id);
            var otpCode = await otpService.GenerateOtpAsync(identifier);

            _ = Task.Run(async () =>
            {
                await emailService.SendOtpEmailAsync(user.Email, otpCode);
            });

            logger.LogInformation("[Service] User created(unconfirmed) and OTP sent: {Email}"
            , clientRegisterDTO.Email);

            logger.LogInformation("[SERVICE] Client registration completed for: {Email}", clientRegisterDTO.Email);

            return new OtpResponseDTO
            {
                Success = true,
                Message = "OTP sent to your email. Please verify to complete registration so you can login successfully."
            };
        }

        public async Task<ClientDTO> VerifyOtpAndCompleteRegistrationAsync(OtpVerificationDTO otpVerificationDTO)
        {
            logger.LogInformation("[Service] Completing registration with OTP for: {Email}", otpVerificationDTO.Email);

            var user = await userManager.FindByEmailAsync(otpVerificationDTO.Email);
            if (user is null)
                throw new UserNotFoundException();

            var identifier = GetOtpIdentifier(user.Id);
            var userId = await otpService.VerifyOtpAsync(identifier, otpVerificationDTO.OtpCode);

            if (!userId)
            {
                throw new InvalidOtpException();
            }

            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
            await userManager.AddToRoleAsync(user, "Client");

            var client = await clientRepository.GetByUserId(user.Id);
            if (client is null)
                throw new UserNotFoundException();

            logger.LogInformation("[Service] Registration completed with OTP verification: {Email}", otpVerificationDTO.Email);

            return new ClientDTO
            {
                Name = client.Name,
                PhoneNumber = user.PhoneNumber!,
                Email = user.Email!,
                Token = "token-ToDo",
                RefreshToken = "refreshToken-ToDo"
            };
        }

        private static string GetOtpIdentifier(string userId) => $"registration_{userId}";
    }
}
