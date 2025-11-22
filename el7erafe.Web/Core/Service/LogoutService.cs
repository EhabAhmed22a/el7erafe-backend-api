
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LogoutDTOs;
using System.IdentityModel.Tokens.Jwt;

namespace Service
{
    public class LogoutService(ITokenBlocklistService _tokenBlocklistService,
                            ILogger<LogoutService> _logger) : ILogoutService
    {
        public async Task<LogoutResponseDto> LogoutAsync(string token)
        {
            try
            {
                _logger.LogInformation("[AUTH] Processing logout request");

                // Extract token expiry
                var tokenHandler = new JwtSecurityTokenHandler();
                if (tokenHandler.CanReadToken(token))
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var expiry = jwtToken.ValidTo;

                    // Revoke the token
                    await _tokenBlocklistService.RevokeTokenAsync(token, expiry);

                    _logger.LogInformation("[AUTH] User logged out successfully. Token revoked until: {Expiry}", expiry);

                    return new LogoutResponseDto
                    {
                        Success = true,
                        Message = "Logged out successfully",
                        LogoutTime = DateTime.UtcNow
                    };
                }

                throw new InvalidOperationException("Invalid JWT token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AUTH] Error during logout");
                throw;
            }
        }
    }
}
