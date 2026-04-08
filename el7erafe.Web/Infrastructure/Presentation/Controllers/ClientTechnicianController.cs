using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;
using ServiceAbstraction;
using Shared.DataTransferObject.NotificationDTOs;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/tcf")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Client, Technician")]
    public class ClientTechnicianController(
        IClientTechnicianCommonService clientTechnicianCommonService,
        IHubContext<ClientHub> clientHub,
        IHubContext<TechnicianHub> technicianHub,
        INotificationService notificationService) : ControllerBase
    {
        [HttpPut("cancelreservation/{reservationId:int}")]
        public async Task<IActionResult> CancelReservation(int reservationId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (userId == null || role == null)
            {
                return Unauthorized(new { message = "غير مصرح لك بالقيام بهذه العملية" });
            }

            var (cancelledResId, targetUserId) = await clientTechnicianCommonService.CancelReservationAsync(reservationId, userId, role);

            if (role == "Client")
            {
                await technicianHub.Clients.User(targetUserId).SendAsync("ReservationCancelled", cancelledResId);
                await notificationService.SendAsync(targetUserId, new NotificationDto
                {
                    Title = "إلغاء الحجز",
                    Body = "قام العميل بإلغاء الحجز",
                    Action = "TECH_CANCELLED",
                    ExtraPayload = new
                    {
                        reservationId = cancelledResId
                    }
                });
            }
            else if (role == "Technician")
            {
                await clientHub.Clients.User(targetUserId).SendAsync("ReservationCancelled", cancelledResId);
                await notificationService.SendAsync(targetUserId, new NotificationDto
                {
                    Title = "تم إلغاء الحجز",
                    Body = "قام الفني بإلغاء الحجز",
                    Action = "CLIENT_CANCELLED",
                    ExtraPayload = new
                    {
                        reservationId = cancelledResId
                    }
                });
            }

            return Ok(new { message = "تم إلغاء الحجز بنجاح" });
        }
    }
}