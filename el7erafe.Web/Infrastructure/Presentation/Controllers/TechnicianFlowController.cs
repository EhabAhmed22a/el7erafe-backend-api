

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/tech")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Technician")]
    public class TechnicianFlowController(ITechnicianService technicianService):ControllerBase
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
    }
}
