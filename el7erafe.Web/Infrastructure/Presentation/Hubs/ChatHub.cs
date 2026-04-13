using DomainLayer.Models.ChatModule.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServiceAbstraction.Chat;
using Shared.DataTransferObject.ChatDTOs;

namespace Service.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Client,Technician")]
    public class ChatHub(IChatService _chatService,
                         ILogger<ChatHub> _logger) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            var connectionId = Context.ConnectionId;
            _logger.LogInformation("=== OnConnectedAsync Started ===");
            _logger.LogInformation("UserId: {UserId}", userId);
            _logger.LogInformation("ConnectionId: {ConnectionId}", connectionId);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("❌ UserId is null or empty - aborting connection");
                Context.Abort();
                return;
            }

            await _chatService.AddUserConnectionAsync(userId, connectionId);
            await _chatService.MarkAllMessagesAsDeliveredAsync(userId);
            await Clients.Others.SendAsync("UserOnline", userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier;

            // JUST remove connection from database - nothing else
            await _chatService.RemoveConnectionAsync(connectionId);

            if (!string.IsNullOrEmpty(userId))
            {
                var isStillOnline =
                    await _chatService.IsUserOnline(userId);

                // 🔥 If no more connections, notify offline
                if (!isStillOnline)
                {
                    await Clients.Others.SendAsync("UserOffline", userId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ========== MESSAGE OPERATIONS ==========

        public async Task SendMessage(SendMessageDto messageDto)
        {
            var senderId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(senderId))
                throw new HubException("Unauthorized");

            // 1️ Save message
            var savedMessage = await _chatService.SendMessageAsync(messageDto,senderId);

            // 2️ Get receiver connections (ChatHub only)
            var receiverConnections = await _chatService.GetUserChatConnectionsAsync(savedMessage.ReceiverId);

            bool isDelivered = receiverConnections.Any();

            // 3️ Update delivery status
            if (isDelivered)
            {
                await _chatService.UpdateMessageStatusAsync(savedMessage.Id,MessageStatus.Delivered);
                savedMessage.MessageStatus = MessageStatus.Delivered.ToString();
            }

            // 4️ Send to sender
            await Clients.Caller.SendAsync("MessageSent", savedMessage);

            if (isDelivered)
            {
                await Clients.Caller.SendAsync(
                    "MessageStatusUpdated",
                    savedMessage.Id,
                    MessageStatus.Delivered.ToString()
                );
            }

            // 5️ Send to receiver
            foreach (var connectionId in receiverConnections)
            {
                await Clients.Client(connectionId)
                    .SendAsync("ReceiveMessage", savedMessage);

                await Clients.Client(connectionId)
                    .SendAsync("MessageStatusUpdated",
                        savedMessage.Id,
                        savedMessage.MessageStatus);
            }

            // 6️ Update inbox
            await SendInboxUpdateToUser(savedMessage.ReceiverId);
            await SendInboxUpdateToUser(senderId);
        }

        public async Task MarkMessagesAsRead(int chatId)
        {
            var userId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(userId))
                throw new HubException("Unauthorized");

            // 1️ Call core logic
            var (updatedMessageIds, otherUserId) =
                await _chatService.MarkMessagesAsReadCoreAsync(chatId, userId);

            if (!updatedMessageIds.Any())
                return;

            // 2️ Get receiver connections
            var otherUserConnections = await _chatService.GetUserChatConnectionsAsync(otherUserId);

            // 3️ Emit event
            foreach (var connectionId in otherUserConnections)
            {
                await Clients.Client(connectionId).SendAsync(
                    "MessagesRead",
                    chatId,
                    updatedMessageIds,
                    MessageStatus.Read.ToString()
                );
            }

            // 4️ Update inbox
            await SendInboxUpdateToUser(userId);
            await SendInboxUpdateToUser(otherUserId);
        }

        // ========== CHAT MANAGEMENT ==========

        private async Task SendInboxUpdateToUser(string userId)
        {
            var inbox = await _chatService.GetInboxAsync(userId);

            var connections = await _chatService.GetUserChatConnectionsAsync(userId);

            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("InboxUpdated", inbox);
            }
        }
    }
}
