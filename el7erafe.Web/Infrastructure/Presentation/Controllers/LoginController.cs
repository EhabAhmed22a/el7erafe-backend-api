using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LoginDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/auth/login")]
    public class LoginController(ILoginService loginService, ILogger<LoginController> logger) : ControllerBase
    {
        /// <summary>
        /// Authenticates a user and returns JWT token with user information.
        /// </summary>
        /// <remarks>
        /// This endpoint handles user authentication for both clients and technicians.
        /// Returns appropriate status codes for different authentication states.
        /// </remarks>
        /// <param name="loginDTO">Login credentials containing phone number and password</param>
        /// <returns>Returns user data with JWT token on successful authentication</returns>
        /// <response code="200">Returns when login is successful with user data and JWT token</response>
        /// <response code="401">Returns when credentials are invalid</response>
        /// <response code="452">Returns when client email is not verified</response>
        /// <response code="460">Returns when technician account is pending admin approval</response>
        /// <response code="461">Returns when technician account is rejected by admin</response>
        /// <response code="500">Returns when internal server error occurs</response>
        [HttpPost]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            logger.LogInformation("[API] Login attempt for: {PhoneNumber}", loginDTO.PhoneNumber);
            return Ok(await loginService.LoginAsync(loginDTO));
        }
    }
}