using DomainLayer.Contracts.ChatModule;
using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
using ServiceAbstraction.Chat;
using Shared.DataTransferObject.ChatDTOs;

namespace Service.Chat
{
    public class ChatService(IChatRepository _chatRepository) : IChatService
    {
        public async Task<ChatDto> GetOrCreateChatAsync(string clientId, string technicianId)
        {
            var chat = await _chatRepository.GetOrCreateChatAsync(clientId, technicianId);
            return new ChatDto
            {
                Id = chat.Id,
                ClientId = chat.ClientId,
                TechnicianId = chat.TechnicianId,
            };
        }

        public async Task<ChatDto?> GetChatByIdAsync(int chatId)
        {
            var chat = await _chatRepository.GetChatByIdAsync(chatId);
            if (chat == null)
                return null;

            return new ChatDto
            {
                Id = chat.Id,
                ClientId = chat.ClientId,
                TechnicianId = chat.TechnicianId
            };
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageDto messageDto,int chatId,string senderId)
        {

            var domainMessage = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                ReceiverId = messageDto.ReceiverId,
                Content = messageDto.Content,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            // Persist and get the created message with its Id and timestamps
            var created = await _chatRepository.AddMessageAsync(domainMessage);

            // Map domain Message -> MessageDto
            return new MessageDto
            {
                Id = created.Id,
                ChatId = created.ChatId,
                SenderId = created.SenderId,
                ReceiverId = created.ReceiverId,
                Content = created.Content,
                CreatedAt = created.CreatedAt,
                IsRead = created.IsRead
            };
        }

        public async Task<IEnumerable<MessageDto>> GetChatHistoryAsync(int chatId, int page = 1, int pageSize = 50)
        {
            var messages = await _chatRepository.GetChatMessagesAsync(chatId, page, pageSize);
            return messages.Select(message => new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead
            });
        }

        public async Task MarkMessagesAsReadAsync(int chatId, string userId)
        {
            await _chatRepository.MarkMessagesAsReadAsync(chatId, userId);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _chatRepository.GetUnreadCountAsync(userId);
        }

        public async Task AnonymizeUserDataAsync(string userId, string deletedMarker)
        {
            await _chatRepository.AnonymizeUserChatsAsync(userId, deletedMarker);
        }
    }
}
