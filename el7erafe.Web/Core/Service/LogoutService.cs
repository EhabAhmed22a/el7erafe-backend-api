
using DomainLayer.Contracts;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LogoutDTOs;
using System.IdentityModel.Tokens.Jwt;

namespace Service
{
    public class LogoutService(ILogger<LogoutService> _logger,
                               IUserTokenRepository _userTokenRepository) : ILogoutService
    {
        public async Task<LogoutResponseDto> LogoutAsync(string userId)
        {
            try
            {
                _logger.LogInformation("[AUTH] Processing logout request");
                await _userTokenRepository.DeleteUserTokenAsync(userId);

                _logger.LogInformation("[AUTH] Logout successful for user: {UserId}", userId);

                return new LogoutResponseDto
                {
                    Success = true,
                    Message = "تم تسجيل الخروج بنجاح",
                    LogoutTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUTH] Error during logout");
                throw;
            }
        }
    }
}
