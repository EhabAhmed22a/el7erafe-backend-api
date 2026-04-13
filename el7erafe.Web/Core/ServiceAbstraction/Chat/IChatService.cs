using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
using Shared.DataTransferObject.ChatDTOs;

namespace ServiceAbstraction.Chat
{
    public interface IChatService
    {
        // Connection management
        Task<UserConnection> AddUserConnectionAsync(string userId, string connectionId);
        Task RemoveConnectionAsync(string connectionId);
        Task<List<string>> GetUserChatConnectionsAsync(string userId);

        // Chat management
        Task<ChatDto> InitChatAsync(string clientId, int reservationId);

        // Message operations
        Task<MessageDto> SendMessageAsync(SendMessageDto messageDto, string senderId);
        Task<IEnumerable<MessageDto>> GetChatHistoryAsync(string userId, int chatId, int page = 1, int pageSize = 50);
        Task<(List<int> UpdatedMessageIds, string OtherUserId)> MarkMessagesAsReadCoreAsync(int chatId, string userId);
        Task MarkAllMessagesAsDeliveredAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task UpdateMessageStatusAsync(int messageId, MessageStatus newStatus);

        // Delete/Anonymize (called when user is deleted)
        Task AnonymizeUserDataAsync(string userId, string deletedMarker);

        Task<IEnumerable<InboxConversationDto>> GetInboxAsync(string userId);

        Task<bool> IsUserOnline(string userId);
    }
}