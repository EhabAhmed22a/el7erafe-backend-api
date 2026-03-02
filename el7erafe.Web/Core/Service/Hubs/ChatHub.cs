using DomainLayer.Contracts.ChatModule;
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

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // JUST remove connection from database - nothing else
            await _userConnectionRepository.RemoveConnectionAsync(connectionId);

            await base.OnDisconnectedAsync(exception);
        }

        // ========== MESSAGE OPERATIONS ==========

        public async Task SendMessage(SendMessageDto messageDto)
        {
            var senderId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(senderId))
                throw new HubException("Unauthorized");

            var chat = await _chatService.GetOrCreateChatAsync(senderId,messageDto.ReceiverId);

            // Save message to database
            var savedMessage = await _chatService.SendMessageAsync(messageDto, chat.Id, senderId);

            // Send to receiver ONLY (if they're online)
            var receiverConnections = await _userConnectionRepository.GetUserConnectionsAsync(messageDto.ReceiverId);

            if (receiverConnections.Any())
            {
                await _chatService.UpdateMessageStatusAsync(savedMessage.Id,MessageStatus.Delivered);
                savedMessage.IsRead = MessageStatus.Delivered.ToString();
            }

            foreach (var connectionId in receiverConnections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", savedMessage);
            }

            // Send confirmation back to sender
            await Clients.Caller.SendAsync("MessageSent", savedMessage);
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
            await _chatService.MarkMessagesAsReadAsync(chatId, userId);

            // Notify the other user
            var otherUserConnections = await _userConnectionRepository.GetUserConnectionsAsync(otherUserId);
            foreach (var connectionId in otherUserConnections)
            {
                await Clients.Client(connectionId).SendAsync("MessagesRead", chatId, userId);
            }
        }

        // ========== CHAT MANAGEMENT ==========

        public async Task<ChatDto> GetOrCreateChat(string clientId, string technicianId)
        {
            var userId = Context.UserIdentifier;

            // Verify user is either the client or technician
            if (userId != clientId && userId != technicianId)
            {
                throw new HubException("Unauthorized");
            }

            // JUST create/get chat - no broadcasting
            return await _chatService.GetOrCreateChatAsync(clientId, technicianId);

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
    }
}
