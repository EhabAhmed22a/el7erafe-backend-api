using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using Microsoft.AspNetCore.Authorization; // ← This is what you're missing

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class TechnicianRegistrationController(ITechAuthenticationService _techAuthenticationService,
        ILogger<TechnicianRegistrationController> _logger) : ControllerBase
    {
        [AllowAnonymous] // ← This will now work
        [HttpPost("register/technician")]
        public async Task<ActionResult<TechDTO>> Register(TechRegisterDTO techRegisterDTO)
        {
            _logger.LogInformation("[API] Registering Technician with phone: {Phone}", techRegisterDTO.PhoneNumber);
            var technician = await _techAuthenticationService.techRegisterAsync(techRegisterDTO);

            _logger.LogInformation("[API] Technician registered successfully with Name: {ClientName}", technician.Name);
            return CreatedAtAction(nameof(Register), technician);
        }
    }
}