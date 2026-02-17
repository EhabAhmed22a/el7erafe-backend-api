using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Client")]
    public class ClientController(IClientService _clientService, ILogger<ClientController> _logger) : ControllerBase
    {
        [HttpGet("/cf/services")]
        public async Task<ActionResult<string>> GetServicesAsync()
        {
            _logger.LogInformation("[CONTROLLER] Getting all services");
            var services = await _clientService.GetClientServicesAsync();
            _logger.LogInformation("[CONTROLLER] successfully Getting all services");
            return Ok(services);
        }

        [HttpPost("cf/reservations/quick")]
        public async Task<IActionResult> QuickReserve([FromForm] ServiceRequestRegDTO requestRegDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await _clientService.QuickReserve(requestRegDTO, userId);
            return Ok(new { message = "تم الحجز بنجاح" });
        }

        [HttpGet("cf/profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var result = await _clientService.GetProfileAsync(userId);
            return Ok(result);
        }

        [HttpPatch("cf/profile_update")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateNameImageDTO update)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await _clientService.UpdateNameAndImage(userId, update);
            return Ok(new { message = "تم تحديث الملف الشخصي بنجاح" });
        }
    }
}
