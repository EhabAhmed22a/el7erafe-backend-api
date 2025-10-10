using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/")]
    public class TechAuthenticationController(ITechAuthenticationService _techAuthenticationService) : ControllerBase
    {
        //Login
        [HttpPost("Technician/Login")]
        public async Task<ActionResult<TechDTO>> Login(TechLoginDTO techLoginDTO)
        {
            var User = await _techAuthenticationService.techLoginAsync(techLoginDTO);
            return Ok(User);
        }

        [HttpPost("Technician/Register")]
        public async Task<ActionResult<TechDTO>> Register(TechRegisterDTO techRegisterDTO)
        {
            var User = await _techAuthenticationService.techRegisterAsync(techRegisterDTO);
            return Ok(User);
        }
    }
}
