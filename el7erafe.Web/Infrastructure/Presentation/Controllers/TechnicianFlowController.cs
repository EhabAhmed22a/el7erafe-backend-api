using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using ServiceAbstraction.Chat;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/tech")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Technician")]
    public class TechnicianFlowController(ITechnicianService technicianService,
                                          IChatService chatService): ControllerBase
    {
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var result = await technicianService.GetProfile(userId);
            return Ok(result);
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await chatService.GetInboxAsync(userId);

            return Ok(result);
        }
    }
}
