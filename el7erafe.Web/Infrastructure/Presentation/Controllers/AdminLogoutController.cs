
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LogoutDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/auth/logout")]
    public class AdminLogoutController(ILogoutService logoutService, ILogger<LogoutController> logger):ControllerBase
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
            logger.LogInformation("[CONTROLLER] Checking approval for user: {UserId}", userId);
            var result = await logoutService.LogoutAsync(userId);

            return Ok(result);
        }
    }
}
