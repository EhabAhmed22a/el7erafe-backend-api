using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class ClientRegistrationController(IClientAuthenticationService service
        , ILogger<ClientRegistrationController> logger) : ControllerBase
    {

        /// <summary>
        /// Initiates the client registration process by creating an unconfirmed user and sending OTP verification.
        /// </summary>
        /// <remarks>
        /// This endpoint creates a user with unconfirmed email status and sends an OTP for verification.
        /// The user cannot login until the OTP is verified via the VerifyOtp endpoint.
        /// </remarks>
        /// <param name="clientRegisterDTO">Client registration data including name, email, phone number, and password.</param>
        /// <returns>Returns OTP response with success status and verification instructions.</returns>
        /// <response code="201">Returns when registration is initiated successfully and OTP is sent</response>
        /// <response code="400">Returns when validation fails (invalid data format)</response>
        /// <response code="409">Returns when email or phone number already exists</response>
        /// <response code="500">Returns when internal server error occurs</response>
        [HttpPost("register/client")]
        public async Task<ActionResult<OtpResponseDTO>> RegisterClient(ClientRegisterDTO clientRegisterDTO)
        {
            logger.LogInformation("[API] Starting registration with OTP for: {Email}", clientRegisterDTO.Email);
            var result = await service.RegisterAndSendOtpAsync(clientRegisterDTO);
            return CreatedAtAction(nameof(RegisterClient), result);
        }

        /// <summary>
        /// Completes the client registration process by verifying the OTP and activating the user account.
        /// </summary>
        /// <remarks>
        /// This endpoint verifies the OTP code (valid only for 3 minutes) sent to the user's email and activates the previously created user account.
        /// Upon successful verification, the user's email is confirmed and they are assigned the Client role.
        /// </remarks>
        /// <param name="otpVerificationDTO">OTP verification data containing email and OTP code.</param>
        /// <returns>Returns complete client profile with authentication tokens.</returns>
        /// <response code="200">Returns when OTP is verified successfully and user is activated</response>
        /// <response code="404">Returns when no user found with the provided email</response>
        /// <response code="406">Returns when OTP is invalid or expired</response>
        /// <response code="500">Returns when internal server error occurs</response>
        [HttpPost("verify-otp")]
        public async Task<ActionResult<ClientDTO>> VerifyOtp(OtpVerificationDTO otpVerificationDTO)
        {
            logger.LogInformation("[API] Completing registration with OTP for: {Email}", otpVerificationDTO.Email);
            var client = await service.VerifyOtpAndCompleteRegistrationAsync(otpVerificationDTO);
            return Ok(client);
        }

        /// <summary>
        /// Resends OTP to the user's email for registration completion.
        /// </summary>
        /// <remarks>
        /// This endpoint resends OTP code to unverified email addresses for users who haven't completed registration.
        /// The OTP can only be resent after 60 seconds have passed since the last OTP was sent.
        /// If the email is already verified or an OTP was sent recently, appropriate errors will be returned.
        /// </remarks>
        /// <param name="resendOtpRequestDTO">Request containing the email address</param>
        /// <returns>Returns OTP response with success status and message</returns>
        /// <response code="200">Returns when OTP is resent successfully</response>
        /// <response code="404">Returns when no user found with the provided email</response>
        /// <response code="409">Returns when email is already verified</response>
        /// <response code="429">Returns when OTP was sent recently (within 60 seconds)</response>
        /// <response code="500">Returns when internal server error occurs</response>
        [HttpPost("resend-otp")]
        public async Task<ActionResult<OtpResponseDTO>> ResendOtp(ResendOtpRequestDTO resendOtpRequestDTO)
        {
            logger.LogInformation("[API] Resending OTP for: {Email}", resendOtpRequestDTO.Email);
            return Ok(await service.ResendOtp(resendOtpRequestDTO));
        }
    }
}