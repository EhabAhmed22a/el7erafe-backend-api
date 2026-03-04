using DomainLayer.Contracts.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MimeKit;
using ServiceAbstraction.Chat;
using Shared.DataTransferObject.ChatDTOs;

namespace Service.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Client,Technician")]
    public class ChatHub(IChatService _chatService,
                         IUserConnectionRepository _userConnectionRepository,
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

            await _userConnectionRepository.AddConnectionAsync(userId, connectionId);
            await _chatService.MarkAllMessagesAsDeliveredAsync(userId);
            await Clients.Others.SendAsync("UserOnline", userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier;

            // JUST remove connection from database - nothing else
            await _userConnectionRepository.RemoveConnectionAsync(connectionId);

            if (!string.IsNullOrEmpty(userId))
            {
                var isStillOnline =
                    await _userConnectionRepository.IsUserOnlineAsync(userId);

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

            var chat = await _chatService.GetOrCreateChatAsync(senderId, messageDto.ReceiverId);

            var savedMessage = await _chatService.SendMessageAsync(messageDto, chat.Id, senderId);

            var receiverConnections = await _userConnectionRepository.GetUserConnectionsAsync(messageDto.ReceiverId);

            bool isDelivered = receiverConnections.Any();

            if (isDelivered)
            {
                await _chatService.UpdateMessageStatusAsync(savedMessage.Id,MessageStatus.Delivered);
                savedMessage.MessageStatus = MessageStatus.Delivered.ToString();
            }

            await Clients.Caller.SendAsync("MessageSent", savedMessage);

            if (isDelivered)
            {
                await Clients.Caller.SendAsync("MessageStatusUpdated",savedMessage.Id,MessageStatus.Delivered.ToString());
            }

            foreach (var connectionId in receiverConnections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", savedMessage);
            }

            await SendInboxUpdateToUser(messageDto.ReceiverId);
            await SendInboxUpdateToUser(senderId);
        }

        public async Task MarkMessagesAsRead(int chatId, string otherUserId)
        {
            var userId = Context.UserIdentifier;

            var chat = await _chatService.GetChatByIdAsync(chatId);
            if (chat == null || (chat.ClientId != userId && chat.TechnicianId != userId))
            {
                throw new HubException("Unauthorized");
            }

            // Mark messages as read
            var updatedMessageIds = await _chatService.MarkMessagesAsReadAsync(chatId, userId);

            // Notify the other user
            if (updatedMessageIds.Any())
            {
                var otherUserConnections =
                    await _userConnectionRepository.GetUserConnectionsAsync(otherUserId);

                foreach (var connectionId in otherUserConnections)
                {
                    await Clients.Client(connectionId).SendAsync(
                        "MessagesRead",
                        chatId,
                        updatedMessageIds,
                        MessageStatus.Read.ToString()
                    );
                }
            }
            await SendInboxUpdateToUser(userId);
            await SendInboxUpdateToUser(otherUserId);
        }

        // ========== CHAT MANAGEMENT ==========

        public async Task<ChatDto> GetOrCreateChat(string receiverId)
        {
            var userId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(userId))
                throw new HubException("Unauthorized");

            // JUST create/get chat
            return await _chatService.GetOrCreateChatAsync(userId, receiverId);
        }

        public async Task<IEnumerable<MessageDto>> GetChatHistory(int chatId, int page = 1, int pageSize = 50)
        {
            var userId = Context.UserIdentifier;

            // Verify user has access
            var chat = await _chatService.GetChatByIdAsync(chatId);
            if (chat == null || (chat.ClientId != userId && chat.TechnicianId != userId))
            {
                throw new HubException("Unauthorized");
            }

            return await _chatService.GetChatHistoryAsync(chatId, page, pageSize);
        }

        private async Task SendInboxUpdateToUser(string userId)
        {
            var inbox = await _chatService.GetInboxAsync(userId);

            var connections = await _userConnectionRepository
                .GetUserConnectionsAsync(userId);

            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId)
                    .SendAsync("InboxUpdated", inbox);
            }
        }
    }
}
