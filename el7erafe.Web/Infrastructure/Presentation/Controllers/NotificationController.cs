using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.NotificationDTOs;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/notify")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Technician, Client")]
    public class NotificationController(
        IUserService userService) : ControllerBase
    {
        [HttpPost("save-fcm-token")]
        public async Task<IActionResult> SaveFcmToken([FromBody] SaveFcmTokenDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await userService.SaveFcmTokenAsync(userId, request.FcmToken);
            return Ok(new { message = "FCM token saved successfully" });
        }

        [HttpDelete("delete-fcm-token")]
        public async Task<IActionResult> DeleteFcmToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await userService.DeleteFcmTokenAsync(userId);

            return Ok(new { message = "FCM token removed successfully" });
        }
    }
}
