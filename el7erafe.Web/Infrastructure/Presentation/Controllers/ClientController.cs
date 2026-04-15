using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Presentation.Hubs;
using ServiceAbstraction;
using ServiceAbstraction.Chat;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.NotificationDTOs;
using Shared.DataTransferObject.OffersDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Client")]
    public class ClientController(IClientService _clientService,
        ITechnicianFlowService technicianService,
        ILogger<ClientController> _logger,
        IChatService chatService,
        IHubContext<ClientHub> clientHub,
        IHubContext<TechnicianHub> technicianHub,
        ITechnicianAvailabilityService technicianAvailabilityService,
        INotificationService notificationService) : ControllerBase
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

            var newData = await _clientService.ServiceRequest(requestRegDTO, userId);
            await clientHub.Clients.User(userId).SendAsync("ReceivePendingRequests", newData);

            var targetTechs = await technicianAvailabilityService.GetAvailableTechnicianByUserIdsAsync(newData.ServiceId, newData.GovernorateId, newData.day ?? DateOnly.MinValue, newData.From, newData.To);
            if (targetTechs.Any())
            {
                await technicianHub.Clients.Users(targetTechs).SendAsync("ReceiveNewQuickRequest", newData);
                await notificationService.SendAsync(targetTechs, new NotificationDto
                {
                    Title = "طلب خدمة جديد",
                    Body = "يوجد طلب خدمة جديد قريب منك",
                    Action = "TECH_NEW_REQUEST",
                    ExtraPayload = new
                    {
                        requestId = newData.requestId,
                        serviceId = newData.ServiceId
                    }
                });
            }

            return Ok(new { message = "تم إرسال طلب الخدمة بنجاح. سيتم تعيين فني قريباً" });
        }

        [HttpGet("cf/getpendingservicerequests")]
        public async Task<IActionResult> GetPendingSerReq()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            return Ok(await _clientService.GetPendingServiceRequestsAsync(userId));
        }

        [HttpPost("cf/select_technician")]
        public async Task<IActionResult> TechnicianReserve([FromForm] ServiceRequestRegDTO requestRegDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var newData = await _clientService.ServiceRequest(requestRegDTO, userId);
            await clientHub.Clients.User(userId).SendAsync("ReceivePendingRequests", newData);

            if (requestRegDTO.TechnicianId.HasValue)
            {
                var technician = await technicianService.GetTechnicianByIdAsync((int)requestRegDTO.TechnicianId);
                await technicianHub.Clients.User(technician?.User.Id!).SendAsync("ReceiveNewDirectRequest", newData);
                await notificationService.SendAsync(technician?.User.Id!, new NotificationDto
                {
                    Title = "طلب خدمة جديد",
                    Body = "تم إرسال طلب خدمة لك من قبل عميل",
                    Action = "TECH_NEW_REQUEST",
                    ExtraPayload = new
                    {
                        requestId = newData.requestId,
                        serviceId = newData.ServiceId
                    }
                });
            }
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
                message = "تم تحديث الاسم و الصورة";
            else if (hasName)
                message = "تم تحديث الاسم";
            else if (hasImage)
                message = "تم تحديث الصورة";
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

        [HttpPatch("cf/cancel-request")]
        public async Task<IActionResult> CancelRequest(ReqIdDTO cancelReqDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var techUserId = await _clientService.CancelRequestAsync(userId, cancelReqDTO);
            if (techUserId is not null)
                await technicianHub.Clients.User(techUserId).SendAsync("RemoveCanceledRequest", cancelReqDTO.requestId);
            else
                await technicianHub.Clients.All.SendAsync("RemoveCanceledRequest", cancelReqDTO.requestId);

            return Ok(new { message = "تم الغاء الطلب بنجاح" });
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

        [HttpGet("cf/offers/quick")]
        public async Task<IActionResult> GetQuickOffers([FromQuery] ReqIdDTO reqIdDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Ok(await _clientService.GetOffersAsync(userId, reqIdDTO.requestId, true));
        }

        [HttpGet("cf/offers/specific")]
        public async Task<IActionResult> GetTechOffers([FromQuery] ReqIdDTO reqIdDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Ok(await _clientService.GetOffersAsync(userId, reqIdDTO.requestId, false));
        }

        [HttpGet("cf/getprevreservations")]
        public async Task<IActionResult> GetPreviousReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Ok(await _clientService.GetPreviousReservations(userId));
        }

        [HttpGet("cf/getcurrentreservations")]
        public async Task<IActionResult> GetCurrentReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Ok(await _clientService.GetCurrentReservationsAsync(userId));
        }

        [HttpPost("cf/offers/accept")]
        public async Task<IActionResult> AcceptOffer([FromBody] OfferIdDto offerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var result = await _clientService.AcceptOffer(offerId.offerId);

            await technicianHub.Clients.User(result.AcceptedTechnicianUserId).SendAsync("OfferAccepted",
                new
                {
                    requestId = result.RequestId,
                    acceptedOfferId = result.AcceptedOfferId
                });

            await notificationService.SendAsync(result.AcceptedTechnicianUserId, new NotificationDto
            {
                Title = "تم قبول العرض",
                Body = "تم قبول عرضك من العميل",
                Action = "TECH_OFFER_ACCEPTED",
                ExtraPayload = new
                {
                    requestId = result.RequestId,
                    offerId = result.AcceptedOfferId
                }
            });

            if (result.RejectedTechnicianUserIds.Any())
            {
                await technicianHub.Clients
                    .Users(result.RejectedTechnicianUserIds)
                    .SendAsync("OfferRejected", new
                    {
                        requestId = result.RequestId,
                        acceptedOfferId = result.AcceptedOfferId
                    });

                await notificationService.SendAsync(result.RejectedTechnicianUserIds, new NotificationDto
                {
                    Title = "تم رفض العرض",
                    Body = "تم اختيار عرض آخر لهذا الطلب",
                    Action = "TECH_OFFER_DECLINED",
                    ExtraPayload = new
                    {
                        requestId = result.RequestId,
                        acceptedOfferId = result.AcceptedOfferId
                    }
                });
            }

            return Ok(new { message = "تم قبول العرض بنجاح" });
        }

        [HttpPost("cf/paynow/{reservationId:int}")]
        public async Task<IActionResult> PayNow(int reservationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "المستخدم غير موجود" });

            var techUserId = await _clientService.PayNow(reservationId);
            await technicianHub.Clients.User(techUserId).SendAsync("PaymentCompleted", reservationId);
            await notificationService.SendAsync(techUserId, new NotificationDto
            {
                Title = "تم استلام الدفع",
                Body = "قام العميل بالدفع بنجاح",
                Action = "TECH_PAYMENT_DONE",
                ExtraPayload = new
                {
                    reservationId = reservationId
                }
            });

            return Ok(new { message = "تمت عملية الدفع بنجاح" });
        }

        [HttpPost("cf/offers/decline")]
        public async Task<IActionResult> DeclineOffer([FromBody] OfferIdDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("المستخدم غير موجود");

            var result = await _clientService.DeclineOffer(dto.offerId);

            await technicianHub.Clients.User(result.TechnicianUserId)
                .SendAsync("OfferRejected", new
                {
                    requestId = result.RequestId,
                    offerId = result.OfferId
                });

            await notificationService.SendAsync(result.TechnicianUserId, new NotificationDto
            {
                Title = "تم رفض العرض",
                Body = "تم رفض عرضك من العميل",
                Action = "TECH_OFFER_DECLINED",
                ExtraPayload = new
                {
                    requestId = result.RequestId,
                    offerId = result.OfferId
                }
            });

            return Ok(new { message = "تم رفض العرض بنجاح" });
        }

        [HttpPost("cf/rate/{reservationId:int}/")]
        public async Task<IActionResult> RateTechnician(int reservationId, [FromQuery] int ratingValue)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "المستخدم غير موجود" });
            }

            await _clientService.SubmitRatingAsync(reservationId, ratingValue, userId);
            return Ok(new { message = "تم تقييم الفني بنجاح، شكراً لك!" });
        }
    }
}
