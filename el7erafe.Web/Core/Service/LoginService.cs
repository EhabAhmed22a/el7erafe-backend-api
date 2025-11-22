using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.LoginDTOs;

namespace Service
{
    public class LoginService(UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IClientRepository clientRepository,
        ITechnicianRepository technicianRepository,
        IOtpService otpService,
        IEmailService emailService,
        ILogger<LoginService> logger) : ILoginService
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

            logger.LogInformation("[SERVICE] Password verification successful for user: {UserId}", user.Id);

            if (user.UserType == UserTypeEnum.Client)
            {
                logger.LogInformation("[SERVICE] Processing client login flow for user: {UserId}", user.Id);

                if (!user.EmailConfirmed)
                {
                    logger.LogWarning("[SERVICE] Client email not verified for user: {UserId}, sending OTP", user.Id);

                    var identifier = GetOtpIdentifier(user.Id);
                    var otpCode = await otpService.GenerateOtp(identifier);

                    logger.LogInformation("[SERVICE] OTP generated for client verification: {UserId}", user.Id);

                    _ = Task.Run(async () =>
                    {
                        await emailService.SendOtpEmailAsync(user.Email!, otpCode);
                    });

                    logger.LogInformation("[SERVICE] OTP email sent to client: {Email}", user.Email);
                    logger.LogWarning("[SERVICE] Throwing UnverifiedClientLogin for user: {UserId}", user.Id);

                    throw new UnverifiedClientLogin();
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
                var token = await new CreateToken(userManager, configuration).CreateTokenAsync(user, tempToken: false);

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
                var technician = await technicianRepository.GetByUserIdAsync(user.Id);
                if (technician is null)
                {
                    logger.LogError("[SERVICE] Technician record not found for user: {UserId}", user.Id);
                    throw new UnauthorizedUserException();
                }

                logger.LogInformation("[SERVICE] Processing technician login flow for user: {UserId}", user.Id);
                if (technician.Status == TechnicianStatus.Pending)
                {
                    logger.LogWarning("[SERVICE] Technician login rejected - status Pending for user: {UserId}", user.Id);
                    throw new PendingTechnicianRequest();
                }
                else if (technician.Status == TechnicianStatus.Rejected)
                {
                    logger.LogWarning("[SERVICE] Technician login rejected - status Rejected for user: {UserId}", user.Id);
                    throw new RejectedTechnician();
                }

                logger.LogInformation("[SERVICE] Technician status approved, proceeding with login for user: {UserId}",
                    user.Id);

                logger.LogInformation("[SERVICE] Generating token for technician: {TechnicianName}", technician.Name);
                var token = await new CreateToken(userManager, configuration).CreateTokenAsync(user, tempToken: false);

                logger.LogInformation("[SERVICE] Technician login completed successfully: {TechnicianName} ({UserId})",
                    technician.Name, user.Id);

                return new UserDTO
                {
                    userId = technician.UserId,
                    userName = technician.Name,
                    type = 'T',
                    token = token
                };
            }
        }

        private static string GetOtpIdentifier(string userId) => $"registration_{userId}";
    }
}