using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.AdminDTOs.LoginDTO;

namespace Service
{
    public class AdminLoginService(
        IAdminRepository adminRepository,
        UserManager<ApplicationUser> userManager,
        IUserTokenRepository userTokenRepository,
        IConfiguration configuration,
        ILogger<AdminLoginService> logger) : IAdminLoginService
    {
        public async Task<AdminDTO> LoginAsync(AdminLoginDTO adminLoginDTO)
        {
            logger.LogInformation("[SERVICE] Admin login attempt started for username: {Username}",
                adminLoginDTO.Username);

            logger.LogInformation("[SERVICE] Looking up admin user by username: {Username}",
                adminLoginDTO.Username);

            var user = await userManager.FindByNameAsync(adminLoginDTO.Username);
            if (user is null)
            {
                logger.LogWarning("[SERVICE] Admin login failed: User not found for username: {Username}",
                    adminLoginDTO.Username);
                throw new UnauthorizedAdminException();
            }

            logger.LogInformation("[SERVICE] User found with ID: {UserId}, UserType: {UserType}, checking password",
                user.Id, user.UserType);

            var result = await userManager.CheckPasswordAsync(user, adminLoginDTO.Password);
            if (!result)
            {
                logger.LogWarning("[SERVICE] Admin login failed: Invalid password for user: {UserId}", user.Id);
                throw new UnauthorizedAdminException();
            }

            logger.LogInformation("[SERVICE] Password verification successful for user: {UserId}", user.Id);

            var userToken = await userTokenRepository.GetUserTokenAsync(user.Id);
            if (userToken is not null && user.UserType == UserTypeEnum.Admin)
            {
                logger.LogWarning("[SERVICE] Admin login rejected: User already logged in. UserId: {UserId}", user.Id);
                throw new AlreadyLoggedInException();
            }

            if (user.UserType != UserTypeEnum.Admin)
            {
                logger.LogWarning("[SERVICE] Admin login denied: Non-admin user attempted admin login. UserId: {UserId}, UserType: {UserType}",
                    user.Id, user.UserType);
                throw new ForbiddenAccessException();
            }

            logger.LogInformation("[SERVICE] User is admin, retrieving admin details for user: {UserId}", user.Id);

            var admin = await adminRepository.GetByUserId(user.Id);
            if (admin is null)
            {
                logger.LogError("[SERVICE] Data integrity error: Admin record not found for user ID: {UserId}", user.Id);
                throw new TechnicalException();
            }

            logger.LogInformation("[SERVICE] Admin details found: {AdminName}, generating token for user: {UserId}",
                admin.Name, user.Id);

            var token = await new CreateToken(userManager, configuration).CreateTokenAsync(user);

            logger.LogInformation("[SERVICE] Token generated successfully, storing user token for user: {UserId}", user.Id);

            await userTokenRepository.CreateUserTokenAsync(new UserToken
            {
                Token = token,
                Type = TokenType.Token,
                UserId = user.Id
            });

            logger.LogInformation("[SERVICE] Admin login completed successfully: {AdminName} ({UserId})",
                admin.Name, user.Id);

            return new AdminDTO
            {
                Token = token,
            };
        }
    }
}