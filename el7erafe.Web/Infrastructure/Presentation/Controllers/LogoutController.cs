using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LogoutDTOs;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/auth/logout")]
    public class LogoutController(ILogoutService _logoutService,
                                   ILogger<LogoutController> _logger) : ControllerBase
    {
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<LogoutResponseDto>> Logout()
        {
            try
            {
                // Extract token from header
                var token = ExtractTokenFromHeader();
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var result = await _logoutService.LogoutAsync(token);

                _logger.LogInformation("[CONTROLLER] User {UserId} logged out successfully", GetCurrentUserId());

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CONTROLLER] Error during logout");
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        private string ExtractTokenFromHeader()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return string.Empty; // or return string.Empty;
            }

            return authHeader.Substring("Bearer ".Length).Trim();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        }
    }
}
