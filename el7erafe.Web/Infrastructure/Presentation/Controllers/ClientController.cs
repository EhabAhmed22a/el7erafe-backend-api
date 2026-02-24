using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Client")]
    public class ClientController(IClientService _clientService, ILogger<ClientController> _logger) : ControllerBase
    {
        [HttpGet("/cf/services")]
        public async Task<ActionResult<string>> GetServicesAsync()
        {
            _logger.LogInformation("[CONTROLLER] Getting all services");
            var services = await _clientService.GetClientServicesAsync();
            _logger.LogInformation("[CONTROLLER] successfully Getting all services");
            return Ok(services);
        }

        [HttpPost("cf/reservations/quick")]
        public async Task<IActionResult> QuickReserve([FromForm] ServiceRequestRegDTO requestRegDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await _clientService.ServiceRequest(requestRegDTO, userId);
            return Ok(new { message = "تم إرسال طلب الخدمة بنجاح. سيتم تعيين فني قريباً" });
        }

        [HttpPost("cf/select_technician")]
        public async Task<IActionResult> TechnicianReserve([FromForm] ServiceRequestRegDTO requestRegDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await _clientService.ServiceRequest(requestRegDTO, userId);
            return Ok(new { message = "تم إرسال طلب الخدمة للفني المحدد. في انتظار قبوله" });
        }

        [HttpDelete("client/account")]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            _logger.LogInformation("[CONTROLLER] Getting UserId From the Token");
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[CONTROLLER] DeleteAccount failed: userId not found in claims");
                return Unauthorized("المستخدم غير موجود");
            }

            _logger.LogInformation("[CONTROLLER] DeleteAccount called for UserId: {UserId}", userId);

            await _clientService.DeleteAccount(userId);

            _logger.LogInformation("[CONTROLLER] DeleteAccount completed for UserId: {UserId}", userId);
            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }

        [HttpGet("cf/profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var result = await _clientService.GetProfileAsync(userId);
            return Ok(result);
        }

        [HttpPost("cf/technicians_available")]
        public async Task<IActionResult> GetAvailableTechnicians(GetAvailableTechniciansRequest requestRegDTO)
        {
            _logger.LogInformation("[CONTROLLER] Getting Available Technicians");
            var technicians = await _clientService.GetAvailableTechniciansAsync(requestRegDTO);
            _logger.LogInformation("[CONTROLLER] Successfully Getting Available Technicians");
            return Ok(technicians);
        }

        [HttpPatch("cf/profile_update")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateNameImageDTO update)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await _clientService.UpdateNameAndImage(userId, update);

            bool hasName = !string.IsNullOrEmpty(update.Name);
            bool hasImage = update.Image is not null && update.Image.Length > 0;
            string message;
            if (hasName && hasImage)
                message = "تم تحديث الاسم و رقم الهاتف";
            else if (hasName)
                message = "تم تحديث الاسم";
            else if (hasImage)
                message = "تم تحديث رقم الهاتف";
            else
                message = "تم التحديث بنجاح";
            return Ok(new { message = message });
        }

        [HttpPatch("cf/phone_update")]
        public async Task<IActionResult> UpdatePhoneNumber(UpdatePhoneDTO dTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await _clientService.UpdatePhoneNumber(userId, dTO);
            return Ok(new { message = "تم تحديث رقم الهاتف بنجاح" });
        }

        [HttpPost("cf/update-pending-email")]
        public async Task<IActionResult> UpdatePendingEmail(UpdateEmailDTO dTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var response = await _clientService.UpdatePendingEmail(userId, dTO);
            return Ok(new { message = response.Message });
        }

        [HttpPost("cf/update-email")]
        public async Task<IActionResult> UpdateEmail(OtpCodeDTO otpCode)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await _clientService.UpdateEmailAsync(userId, otpCode);

            return Ok(new { message = "تم تحديث البريد الإلكتروني بنجاح" });
        }

        [HttpPost("cf/resend-otp-pendingemail")]
        public async Task<IActionResult> ResendPendingOtp()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var response = await _clientService.ResendOtpForPendingEmail(userId);

            return Ok(new { message = response.Message });
        }
    }
}
