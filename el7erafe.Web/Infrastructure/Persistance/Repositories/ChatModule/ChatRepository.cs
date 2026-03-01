using DomainLayer.Contracts.ChatModule;
using DomainLayer.Models.ChatModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories.ChatModule
{
    public class ChatRepository(ApplicationDbContext dbContext) : IChatRepository
    {
        // ========== CHAT OPERATIONS ==========
        public async Task<Chat> GetOrCreateChatAsync(string clientId, string technicianId)
        {
            var chat = await dbContext.Chats
                .Include(c => c.Client)
                .Include(c => c.Technician)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.TechnicianId == technicianId);

            if (chat != null)
                return chat;

            var newChat = new Chat
            {
                ClientId = clientId,
                TechnicianId = technicianId
            };

            return await CreateChatAsync(newChat);
        }
        public async Task<Chat?> GetChatByIdAsync(int id)
        {
            return await dbContext.Chats
                .Include(c => c.Client)
                .Include(c => c.Technician)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            // Check if chat already exists between these two
            var existingChat = await dbContext.Chats
                .FirstOrDefaultAsync(c => c.ClientId == chat.ClientId && c.TechnicianId == chat.TechnicianId);

            if (existingChat != null)
            {
                return existingChat;
            }

            await dbContext.Chats.AddAsync(chat);
            await dbContext.SaveChangesAsync(); 
            return chat;
        }
        public async Task<IEnumerable<Chat>> GetUserChatsWithDetailsAsync(string userId)
        {
            return await dbContext.Chats
                .Where(c => c.ClientId == userId || c.TechnicianId == userId)
                .Include(c => c.Client)
                .Include(c => c.Technician)
                .Include(c => c.Messages)
                .ToListAsync();
        }

        // ========== MESSAGE OPERATIONS ==========

        public async Task<IEnumerable<Message>> GetChatMessagesAsync(int chatId, int page = 1, int pageSize = 20)
        {
            return await dbContext.Messages
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message?> GetMessageByIdAsync(int id)
        {
            return await dbContext.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Chat)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<Message> AddMessageAsync(Message message)
        {
            await dbContext.Messages.AddAsync(message);
            await dbContext.SaveChangesAsync(); 
            return message;
        }

        public async Task UpdateMessageAsync(Message message)
        {
            dbContext.Messages.Update(message);
            await dbContext.SaveChangesAsync(); 
        }

        public async Task MarkMessagesAsReadAsync(int chatId, string userId)
        {
            await dbContext.Messages
                .Where(m => m.ChatId == chatId && m.ReceiverId == userId && !m.IsRead)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.IsRead, true));
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await dbContext.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted);
        }

        // ========== DELETE/ANONYMIZE OPERATIONS ==========

        public async Task AnonymizeUserChatsAsync(string userId, string deletedMarker)
        {
            // Update chats where user is client
            await dbContext.Chats
                .Where(c => c.ClientId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.ClientId, deletedMarker));

            // Update chats where user is technician
            await dbContext.Chats
                .Where(c => c.TechnicianId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.TechnicianId, deletedMarker));
        }

        public async Task AnonymizeUserMessagesAsync(string userId, string deletedMarker)
        {
            // Update messages where user is sender
            await dbContext.Messages
                .Where(m => m.SenderId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.SenderId, deletedMarker));

            // Update messages where user is receiver
            await dbContext.Messages
                .Where(m => m.ReceiverId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.ReceiverId, deletedMarker));
        }

    }
}
