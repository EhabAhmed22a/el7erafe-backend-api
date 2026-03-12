

using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using ServiceAbstraction.Chat;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using Shared.DataTransferObject.TechnicianSchedule;
using Shared.DataTransferObject.UpdateDTOs;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/tech")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Technician")]
    public class TechnicianFlowController(
        ITechnicianService technicianService,
        IChatService chatService,
        ITechnicianAvailabilityService technicianAvailabilityService) : ControllerBase
    {
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var result = await technicianService.GetProfile(userId);
            return Ok(result);
        }

        [HttpPatch("profile_update")]
        public async Task<IActionResult> UpdateTechnicianProfile([FromForm] UpdateTechnicianDTO update)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await technicianService.UpdateBasicInfo(userId, update);

            // Check what was updated
            bool hasName = !string.IsNullOrEmpty(update.Name);
            bool hasAboutMe = !string.IsNullOrEmpty(update.AboutMe);
            bool hasProfileImage = update.ProfileImage is not null;
            bool hasNewPortfolio = update.NewPortifolioImages is not null && update.NewPortifolioImages.Count > 0;
            bool hasDeletedPortfolio = update.DeletedPortifolioImages is not null && update.DeletedPortifolioImages.Count > 0;

            string message;

            if (hasName && hasAboutMe && (hasProfileImage || hasNewPortfolio || hasDeletedPortfolio))
                message = "تم تحديث جميع البيانات الشخصية بنجاح";
            else if (hasName && hasAboutMe)
                message = "تم تحديث الاسم والوصف الشخصي بنجاح";
            else if (hasName && hasProfileImage)
                message = "تم تحديث الاسم والصورة الشخصية بنجاح";
            else if (hasAboutMe && hasProfileImage)
                message = "تم تحديث الوصف الشخصي والصورة بنجاح";
            else if (hasName)
                message = "تم تحديث الاسم بنجاح";
            else if (hasAboutMe)
                message = "تم تحديث الوصف الشخصي بنجاح";
            else if (hasProfileImage)
                message = "تم تحديث الصورة الشخصية بنجاح";
            else if (hasNewPortfolio && hasDeletedPortfolio)
                message = "تم تحديث معرض الصور بنجاح";
            else if (hasNewPortfolio)
                message = "تم إضافة الصور الجديدة إلى المعرض بنجاح";
            else if (hasDeletedPortfolio)
                message = "تم حذف الصور المحددة من المعرض بنجاح";
            else
                message = "تم التحديث بنجاح";

            return Ok(new { message });
        }

        [HttpPatch("phone_update")]
        public async Task<IActionResult> UpdatePhoneNumber(UpdatePhoneDTO dTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await technicianService.UpdatePhoneNumber(userId, dTO);
            return Ok(new { message = "تم تحديث رقم الهاتف بنجاح" });
        }

        [HttpPost("update-pending-email")]
        public async Task<IActionResult> UpdatePendingEmail(UpdateEmailDTO dTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var response = await technicianService.UpdatePendingEmail(userId, dTO);
            return Ok(new { message = response.Message });
        }

        [HttpPost("update-email")]
        public async Task<IActionResult> UpdateEmail(OtpCodeDTO otpCode)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await technicianService.UpdateEmailAsync(userId, otpCode);

            return Ok(new { message = "تم تحديث البريد الإلكتروني بنجاح" });
        }

        [HttpPost("resend-otp-pendingemail")]
        public async Task<IActionResult> ResendPendingOtp()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var response = await technicianService.ResendOtpForPendingEmail(userId);

            return Ok(new { message = response.Message });
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            await technicianService.DeleteAccount(userId);
            return Ok(new { message = "تم حذف الحساب بنجاح" });
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await chatService.GetInboxAsync(userId);

            return Ok(result);
        }

        [HttpPost("availability/set")]
        public async Task<IActionResult> SetAvailability(List<AvailabilityBlockDto> blocks)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            await technicianAvailabilityService.CreateScheduleAsync(userId, blocks);
            return Ok(new { message = "تم ضبط وقت التوفر بنجاح" });

        }

        [HttpPatch("availability/update")]
        public async Task<IActionResult> UpdateAvailability(int id,[FromBody] UpdateTechnicianAvailabilityDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            await technicianAvailabilityService.UpdateAsync(userId, dto);
            return Ok(new { message = "تم تحديث وقت التوفر بنجاح" });
        }

        [HttpDelete("availability/delete/{id}")]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
                return Unauthorized();

            await technicianAvailabilityService.DeleteTechnicianAvailableTimeAsync(userId, id);

            return Ok(new { message = "تم حذف وقت التوفر بنجاح" });
        }

        [HttpGet("availability")]
        public async Task<ActionResult<List<TechnicianAvailabilityResponseDto>>> GetAvailability()
        {
            var technicianId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (technicianId is null)
                return Unauthorized();

            var result = await technicianAvailabilityService.GetTechnicianAvailableTimeAsync(technicianId);

            return Ok(result);
        }
    }
}