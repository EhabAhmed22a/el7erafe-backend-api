using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    public class TechnicianController(ITechAuthenticationService _techAuthenticationService,
        ILogger<TechnicianController> _logger) : ControllerBase
    {
        [HttpPost("auth/register/technician")]
        public async Task<ActionResult<TechDTO>> Register(TechRegisterDTO techRegisterDTO)
        {
            _logger.LogInformation("[API] Registering Technician with phone: {Phone}", techRegisterDTO.PhoneNumber);
            var technician = await _techAuthenticationService.techRegisterAsync(techRegisterDTO);

            _logger.LogInformation("[API] Technician registered successfully");
            return CreatedAtAction(nameof(Register), technician);
        }

        [HttpGet("technician/check-approval")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult<UserDTO>> CheckApproval()
        {
            // Get user ID from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            _logger.LogInformation("[CONTROLLER] Checking approval for user: {UserId}", userId);
            var result = await _techAuthenticationService.CheckTechnicianApprovalAsync(userId);

            return Ok(result);
        }

        [HttpPatch("technician/resubmit-documents")]
        public async Task<ActionResult<TechResubmitResponseDTO>> Resubmission(TechResubmitDTO techResubmitDTO)
        {
            _logger.LogInformation("[CONTROLLER] Technician Resubmission with phone: {phone}", techResubmitDTO.PhoneNumber);
            var result = await _techAuthenticationService.TechnicianResubmitDocumentsAsync(techResubmitDTO);
            return Ok(result);
        }
    }
}