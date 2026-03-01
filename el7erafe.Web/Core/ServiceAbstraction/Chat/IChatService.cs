using Shared.DataTransferObject.ChatDTOs;

namespace ServiceAbstraction.Chat
{
    public interface IChatService
    {
        // Chat management
        Task<ChatDto> GetOrCreateChatAsync(string clientId, string technicianId);
        Task<ChatDto?> GetChatByIdAsync(int chatId);

        // Message operations
        Task<MessageDto> SendMessageAsync(SendMessageDto messageDto, int chatId, string senderId);
        Task<IEnumerable<MessageDto>> GetChatHistoryAsync(int chatId, int page = 1, int pageSize = 50);
        Task MarkMessagesAsReadAsync(int chatId, string userId);
        Task<int> GetUnreadCountAsync(string userId);

        // Delete/Anonymize (called when user is deleted)
        Task AnonymizeUserDataAsync(string userId, string deletedMarker);

        Task<IEnumerable<InboxConversationDto>> GetInboxAsync(string userId);
    }
}
