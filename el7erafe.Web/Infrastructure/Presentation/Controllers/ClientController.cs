using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ServiceRequestDTOs;

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
            return Ok(new {message = "تم الحجز بنجاح"});
        }

        [HttpDelete("client/account")]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            _logger.LogInformation("[CONTROLLER] Getting UserId From the Token");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[CONTROLLER] DeleteAccount failed: userId not found in claims");
                return Unauthorized("المستخدم غير موجود");
            }

            _logger.LogInformation("[CONTROLLER] DeleteAccount called for UserId: {UserId}", userId);

            await _clientService.DeleteAccount(userId);

            _logger.LogInformation("[CONTROLLER] DeleteAccount completed for UserId: {UserId}", userId);
            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }
    }
}
