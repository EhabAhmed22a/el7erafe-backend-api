using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LoginController(ILoginService loginService,IClientAuthenticationService clientAuthenticationService, ILogger<LoginController> logger) : ControllerBase
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
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            logger.LogInformation("[API] Login attempt for: {PhoneNumber}", loginDTO.PhoneNumber);
            return Ok(await loginService.LoginAsync(loginDTO));
        }

        /// <summary>
        /// Resends OTP to the user's email for registration completion.
        /// </summary>
        /// <remarks>
        /// This endpoint resends OTP code to email addresses for users
        /// The OTP can only be resent after 60 seconds have passed since the last OTP was sent.
        /// If an OTP was sent recently to an email, appropriate errors will be returned.
        /// </remarks>
        /// <param name="forgetPasswordDTO">Request containing the email address</param>
        /// <returns>Returns OTP response with success status and message</returns>
        /// <response code="200">Returns when OTP is resent successfully</response>
        /// <response code="404">Returns when no user found with the provided email</response>
        /// <response code="429">Returns when OTP was sent recently (within 60 seconds)</response>
        /// <response code="500">Returns when internal server error occurs</response>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<OtpResponseDTO>> ForgetPasswordAsync(ResendOtpRequestDTO forgetPasswordDTO)
        {
            logger.LogInformation("[API] Forget Password attempt for: {Email}", forgetPasswordDTO.Email);
            return Ok(await clientAuthenticationService.ResendOtp(forgetPasswordDTO));
        }

        /// <summary>
        /// Verifies the OTP code sent to the user's email for authentication.
        /// </summary>
        /// <remarks>
        /// This endpoint validates the OTP code sent to the user's email address.
        /// The OTP must be a 6-digit numeric code and is validated against the stored OTP for the user.
        /// If the OTP is invalid or expired, an error will be returned.
        /// Successful verification confirms the user's identity for the intended operation.
        /// </remarks>
        /// <param name="otpVerificationDTO">Request containing email and OTP code</param>
        /// <returns>Returns success status upon successful OTP verification</returns>
        /// <response code="200">Returns when OTP is verified successfully</response>
        /// <response code="400">Returns when OTP code is invalid or expired</response>
        /// <response code="404">Returns when no user found with the provided email</response>
        /// <response code="500">Returns when internal server error occurs</response>
        [HttpPost("verify-reset-otp")]
        public async Task<ActionResult> VerifyOtp(OtpVerificationDTO otpVerificationDTO)
        {
            logger.LogInformation("[API] OTP verification attempt for email: {Email}", otpVerificationDTO.Email);
            return Ok(await clientAuthenticationService.VerifyResetOtpAsync(otpVerificationDTO));
        }
    }
}