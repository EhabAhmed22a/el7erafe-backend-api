using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LookupDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class PublicController(ILookupService _lookupService,
                    ILogger<PublicController> _logger) : ControllerBase
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
    }
}