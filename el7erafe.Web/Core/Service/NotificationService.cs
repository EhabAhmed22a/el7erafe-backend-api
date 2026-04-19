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

            if (string.IsNullOrEmpty(user.FcmToken))
                return;

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

            await SendPush(user.FcmToken, dto);
        }

        public async Task SendAsync(List<string> userIds, NotificationDto dto)
        {
            foreach (var userId in userIds)
            {
                await SendAsync(userId, dto);
            }
        }

        private async Task SendPush(string token, NotificationDto dto)
        {
            try
            {
                // 1. Failsafe: Don't crash if Firebase didn't load
                if (FirebaseMessaging.DefaultInstance == null) return;

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

                // 2. The actual send attempt
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (FirebaseMessagingException ex)
            {
                // 3. This catches the "Requested entity was not found" error (Dead Token)
                Console.WriteLine($"Firebase Error (Token likely dead): {ex.Message}");
            }
            catch (Exception ex)
            {
                // 4. Catches any other weird errors to protect the API
                Console.WriteLine($"General Push Failed: {ex.Message}");
            }
        }
    }
}
