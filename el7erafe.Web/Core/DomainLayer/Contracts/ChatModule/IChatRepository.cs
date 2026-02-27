using DomainLayer.Models.ChatModule;

namespace DomainLayer.Contracts.ChatModule
{
    public interface IChatRepository
    {
        Task<Chat?> GetChatByIdAsync(int id);
        Task<Chat> CreateChatAsync(Chat chat);

        // Message operations within chat
        Task<IEnumerable<Message>> GetChatMessagesAsync(int chatId, int page = 1, int pageSize = 50);
        Task<Message?> GetMessageByIdAsync(int id);
        Task<Message> AddMessageAsync(Message message);
        Task UpdateMessageAsync(Message message);
        Task MarkMessagesAsReadAsync(int chatId, string userId);
        Task<int> GetUnreadCountAsync(string userId);

        // Delete operations (for hard delete scenario)
        Task AnonymizeUserChatsAsync(string userId, string deletedMarker);
        Task AnonymizeUserMessagesAsync(string userId, string deletedMarker);
    }
}
