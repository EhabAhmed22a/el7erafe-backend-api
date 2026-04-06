using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Identity;
using ServiceAbstraction;
using Shared.DataTransferObject.NotificationDTOs;
using System.Text.Json;
namespace Service
{
    public class NotificationService(
        INotificationRepository notificationRepository,
        UserManager<ApplicationUser> userManager) : INotificationService
    {
        public async Task SendAsync(string userId, NotificationDto dto)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            // 🔥 check settings
            if (!user.NotificationsEnabled || string.IsNullOrEmpty(user.FcmToken))
                return;

            // 1️⃣ Save to DB via repo
            var notification = new DomainLayer.Models.Notification()
            {
                UserId = userId,
                Title = dto.Title,
                Body = dto.Body,
                Action = dto.Action,
                ExtraPayload = dto.ExtraPayload != null
                    ? JsonSerializer.Serialize(dto.ExtraPayload)
                    : null
            };

            await notificationRepository.AddAsync(notification);

            // 2️⃣ Send FCM
            await SendPush(user.FcmToken, dto);
        }

        private async Task SendPush(string token, NotificationDto dto)
        {
            var payload = dto.ExtraPayload != null
                ? JsonSerializer.Serialize(dto.ExtraPayload)
                : "";

            var message = new Message()
            {
                Token = token,

                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = dto.Title,
                    Body = dto.Body
                },

                Data = new Dictionary<string, string>
            {
                { "action", dto.Action },
                { "payload", payload }
            }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}
