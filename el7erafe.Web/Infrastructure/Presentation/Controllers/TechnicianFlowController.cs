

using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;
using ServiceAbstraction;
using ServiceAbstraction.Chat;
using Shared.DataTransferObject.Calendar;
using Shared.DataTransferObject.NotificationDTOs;
using Shared.DataTransferObject.OffersDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
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
        ITechnicianFlowService technicianService,
        IChatService chatService,
        IHubContext<ClientHub> clientHub,
        IHubContext<TechnicianHub> technicianHub,
        ITechnicianAvailabilityService technicianAvailabilityService,
        IOfferService offerService,
        IClientService clientService,
        INotificationService notificationService) : ControllerBase
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

        [HttpPost("availability/set")]
        public async Task<IActionResult> SetAvailability(List<AvailabilityBlockDto> blocks)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            await technicianAvailabilityService.CreateScheduleAsync(userId, blocks);
            return Ok(new { message = "تم ضبط وقت التوفر بنجاح" });

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

        [HttpGet("getavailablerequests")]
        public async Task<IActionResult> GetAvailableRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            return Ok(await technicianService.GetAvailableRequests(userId));
        }

        [HttpPatch("decline-request")]
        public async Task<IActionResult> DeclineRequest(ReqIdDTO cancelReqDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var clientUserId = await technicianService.DeclineRequestAsync(userId, cancelReqDTO);
            if (!string.IsNullOrEmpty(clientUserId))
                await clientHub.Clients.User(clientUserId)
                                       .SendAsync("RequestRejected", cancelReqDTO.requestId);
            return Ok(new { message = "تم رفض الطلب" });
        }

        [HttpPost("make-offer")]
        public async Task<IActionResult> MakeOffer(MakeOfferDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var result = await offerService.MakeOfferAsync(dto, userId);

            await clientHub.Clients.User(result.ClientUserId).SendAsync("ReceiveNewOffer", result.ClientOffer);

            await notificationService.SendAsync(result.ClientUserId, new NotificationDto
            {
                Title = "عرض جديد",
                Body = "قام فني بإرسال عرض على طلبك",
                Action = "CLIENT_NEW_OFFER",
                ExtraPayload = new
                {
                    requestId = result.ClientOffer.RequestId,
                    offerId = result.ClientOffer.OfferId
                }
            });

            await technicianHub.Clients.User(userId).SendAsync("ReceivePendingOffer", result.TechnicianOffer);

            return Ok(new { message = "تم تقديم العرض بنجاح" });
        }

        [HttpGet("pending-offers")]
        public async Task<IActionResult> GetPendingOffers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await technicianService.GetPendingOffersAsync(userId);

            return Ok(result);
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendar(DateTime? date)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");
            var result = await technicianService.GetCalendar(userId, date ?? DateTime.Now);
            return Ok(result);
        }

        [HttpPost("start-job")]
        public async Task<IActionResult> StartJob([FromBody] ReservationIdDto reservationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var clientUserId = await technicianService.StartJob(userId, reservationId.ReservationId);

            await clientHub.Clients.User(clientUserId).SendAsync("JobStarted",  new { reservationId });

            await notificationService.SendAsync(clientUserId, new NotificationDto
            {
                Title = "تم بدء العمل",
                Body = "بدأ الفني العمل على طلبك",
                Action = "CLIENT_STATUS_CHANGED",
                ExtraPayload = new
                {
                    reservationId = reservationId.ReservationId
                }
            });

            return Ok(new {message = "تم بدء العمل" });
        }

        [HttpGet("in-progress")]
        public async Task<IActionResult> GetInProgress()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var result = await technicianService.GetInProgressReservations(userId);

            return Ok(result);
        }

        [HttpGet("previous-jobs")]
        public async Task<IActionResult> GetPreviousJobs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "المستخدم غير موجود" });
            }

            return Ok(await technicianService.GetPreviousJobsAsync(userId));
        }

        [HttpPost("complete-job")]
        public async Task<IActionResult> CompleteJob([FromBody] ReservationIdDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var clientUserId = await technicianService.CompleteJob(userId, dto.ReservationId);

            await clientHub.Clients.User(clientUserId).SendAsync("JobCompleted", new { dto.ReservationId });

            await notificationService.SendAsync(clientUserId, new NotificationDto
            {
                Title = "تم إنهاء العمل",
                Body = "تم الانتهاء من طلبك بنجاح، برجاء إتمام عملية الدفع",
                Action = "CLIENT_STATUS_CHANGED",
                ExtraPayload = new
                {
                    reservationId = dto.ReservationId
                }
            });

            return Ok(new { message = "تم إنهاء العمل بنجاح" });
        }
    }
}