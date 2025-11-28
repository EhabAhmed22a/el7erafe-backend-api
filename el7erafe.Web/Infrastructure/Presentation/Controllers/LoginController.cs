using System.Security.Claims;
using DomainLayer.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LoginController(ILoginService loginService, IClientAuthenticationService clientAuthenticationService, ILogger<LoginController> logger) : ControllerBase
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

        /// <summary>
        /// Resets the user's password using a temporary reset token.
        /// </summary>
        /// <remarks>
        /// This endpoint allows users to reset their password using a valid temporary reset token.
        /// The token must be issued within the last 5 minutes and is automatically validated from the Authorization header.
        /// The new password must be different from the current password and meet the application's password policy requirements.
        /// Upon successful password reset, the temporary token is invalidated and a new authentication token is issued.
        /// </remarks>
        /// <param name="resetPasswordDTO">Request containing the new password</param>
        /// <returns>Returns user details with new authentication token upon successful password reset</returns>
        /// <response code="200">Returns when password is reset successfully with new token</response>
        /// <response code="400">Returns when the reset token is invalid or missing timestamp</response>
        /// <response code="401">Returns when the bearer token is invalid or expired</response>
        /// <response code="403">Returns when the reset token has expired (older than 5 minutes)</response>
        /// <response code="422">Returns when the new password is identical to the current password</response>
        /// <response code="500">Returns when internal server error occurs</response>
        [HttpPost("reset-password")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token" });

            var iatClaim = User.FindFirst("iat")?.Value;
            if (string.IsNullOrEmpty(iatClaim))
                return Unauthorized(new { message = "Token missing timestamp" });

            var tokenIssuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(iatClaim)).UtcDateTime;
            var currentTime = DateTime.UtcNow;
            return Ok(await loginService.ResetPasswordAsync(resetPasswordDTO, userId, currentTime - tokenIssuedAt));
        }
    }
}