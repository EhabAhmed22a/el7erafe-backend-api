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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            _logger.LogInformation("[CONTROLLER] Checking approval for user: {UserId}", userId);
            var result = await _logoutService.LogoutAsync(userId);

            return Ok(result);
        }
    }
}
