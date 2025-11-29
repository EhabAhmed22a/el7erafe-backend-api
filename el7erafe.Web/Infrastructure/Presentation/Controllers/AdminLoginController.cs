
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.AdminDTOs.LoginDTO;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminLoginController(IAdminLoginService adminLoginService, ILogger<AdminLoginController> logger) : ControllerBase
    {
        /// <summary>
        /// Authenticates an admin user and returns JWT token.
        /// </summary>
        /// <remarks>
        /// This endpoint handles admin authentication and returns a JWT token upon successful login.
        /// Validates admin credentials and checks for existing active sessions.
        /// </remarks>
        /// <param name="adminLoginDTO">Admin login credentials containing username and password</param>
        /// <returns>Returns admin data with JWT token on successful authentication</returns>
        /// <response code="200">Returns when admin login is successful with JWT token</response>
        /// <response code="401">Returns when admin credentials are invalid</response>
        /// <response code="403">Returns when non-admin user attempts to access admin endpoint</response>
        /// <response code="409">Returns when admin is already logged in with an active session</response>
        /// <response code="500">Returns when internal server error occurs</response>
        [HttpPost("login")]
        public async Task<ActionResult> AdminLoginAsync(AdminLoginDTO adminLoginDTO)
        {
            logger.LogInformation("[API] AdminLogin endpoint called for username: {Username}", adminLoginDTO.Username);

            logger.LogInformation("[API] Calling adminLoginService.LoginAsync for username: {Username}", adminLoginDTO.Username);

            return Ok(await adminLoginService.LoginAsync(adminLoginDTO));
        }
    }
}
