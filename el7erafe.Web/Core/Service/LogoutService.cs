
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LogoutDTOs;

namespace Service
{
    public class LogoutService(ILogger<LogoutService> _logger,
                               IUserTokenRepository _userTokenRepository) : ILogoutService
    {
        public async Task<LogoutResponseDto> LogoutAsync(string userId)
        {
            _logger.LogInformation("[AUTH] Processing logout request");
            var logout = await _userTokenRepository.GetUserTokenAsync(userId);
            if (logout?.Type != TokenType.Token)
            {
                _logger.LogWarning("[AUTH] Invalid token type for logout for user: {UserId}", userId);
                throw new UnauthorizedLogoutException("نوع الجلسة غير صالحة لتسجيل الخروج");
            }

            await _userTokenRepository.DeleteUserTokenAsync(userId);
            _logger.LogInformation("[AUTH] Logout successful for user: {UserId}", userId);

            return new LogoutResponseDto
            {
                Success = true,
                Message = "تم تسجيل الخروج بنجاح",
                LogoutTime = DateTime.UtcNow
            };
        }
    }
}

