using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    public class ClientController(IClientService _clientService, ILogger<ClientController> _logger) : ControllerBase
    {
        [HttpGet("/cf/services")]
        public async Task<ActionResult<string>> GetServicesAsync()
        {
            try
            {
                _logger.LogInformation("[CONTROLLER] Getting all services");
                var services = await _clientService.GetClientServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CONTROLLER] Error while getting client services");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}
