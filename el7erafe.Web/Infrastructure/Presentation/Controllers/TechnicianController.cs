using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using Microsoft.AspNetCore.Authorization; // ← This is what you're missing

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    public class TechnicianController(ITechAuthenticationService _techAuthenticationService,
        ILogger<TechnicianController> _logger) : ControllerBase
    {
        [AllowAnonymous] // ← This will now work
        [HttpPost("auth/register/technician")]
        public async Task<ActionResult<TechDTO>> Register(TechRegisterDTO techRegisterDTO)
        {
            _logger.LogInformation("[API] Registering Technician with phone: {Phone}", techRegisterDTO.PhoneNumber);
            var technician = await _techAuthenticationService.techRegisterAsync(techRegisterDTO);

            _logger.LogInformation("[API] Technician registered successfully");
            return CreatedAtAction(nameof(Register), technician);
        }
    }
}