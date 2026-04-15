using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;
using ServiceAbstraction;
using ServiceAbstraction.Chat;
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
        IChatService chatService,
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

        [HttpPost("chat/init/{reservationId:int}")]
        public async Task<IActionResult> InitChat(int reservationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var chat = await chatService.InitChatAsync(userId, reservationId);

            return Ok(chat);
        }

        [HttpGet("chat/history/{chatId:int}")]
        public async Task<IActionResult> GetChatHistory(int chatId, int page = 1, int pageSize = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var chatHistory = await chatService.GetChatHistoryAsync(userId, chatId, page, pageSize);

            return Ok(chatHistory);
        }

        [HttpGet("chat/inbox")]
        public async Task<IActionResult> GetInbox()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await chatService.GetInboxAsync(userId);

            return Ok(result);
        }
    }
}