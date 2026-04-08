using DomainLayer.Models.ChatModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LookupDTOs;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class PublicController(
        ILookupService _lookupService,
        ILogger<PublicController> _logger ) : ControllerBase
    {
        [HttpGet("services")]
        public async Task<ActionResult<IEnumerable<ServicesDto>>> GetServices()
        {
            try
            {
                _logger.LogInformation("[CONTROLLER] Getting all services");
                var services = await _lookupService.GetAllServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CONTROLLER] Error getting services");
                return StatusCode(500, new { message = "An error occurred while retrieving services" });
            }
        }

        [HttpPost("test-fcm")]
        public async Task<IActionResult> TestFcm([FromBody] string token)
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                    return BadRequest("Firebase is NOT initialized");

                var message = new FirebaseAdmin.Messaging.Message()
                {
                    Token = token,

                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = "رساله",
                        Body = "الرسالة دي مكتوبة بالعربي"
                    },

                    Data = new Dictionary<string, string>
            {
                { "action", "TEST" }
            }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

                return Ok(new
                {
                    success = true,
                    messageId = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}