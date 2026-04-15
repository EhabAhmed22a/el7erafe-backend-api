using DomainLayer.Contracts.ChatModule;
using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories.ChatModule
{
    public class UserConnectionRepository(ApplicationDbContext dbContext) : IUserConnectionRepository
    {
        public async Task<UserConnection> AddConnectionAsync(string userId, string connectionId, HubType hubType)
        {
            // Create new connection
            var connection = new UserConnection
            {
                UserId = userId,
                ConnectionId = connectionId,
                ConnectedAt = DateTime.UtcNow,
                HubType = hubType
            };

            await dbContext.UserConnections.AddAsync(connection);
            await dbContext.SaveChangesAsync();

            return connection;
        }

        public async Task<UserConnection?> GetConnectionByIdAsync(string connectionId)
        {
            return await dbContext.UserConnections
                         .Include(uc => uc.User)
                         .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId);
        }

        public async Task<List<string>> GetUserConnectionsAsync(string userId)
        {
            return await dbContext.UserConnections
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ConnectionId)
                .ToListAsync();
        }

        public async Task<bool> IsUserOnlineAsync(string userId, HubType hubType)
        {
            return await dbContext.UserConnections
                .AnyAsync(uc => uc.UserId == userId && uc.HubType == hubType);
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            var connection = await dbContext.UserConnections
                .FirstOrDefaultAsync(uc => uc.ConnectionId == connectionId);

            if (connection is not null)
            {
                // If you want to hard delete instead:
                dbContext.UserConnections.Remove(connection);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetUserConnectionsByTypeAsync(string userId, HubType hubType)
        {
            return await dbContext.UserConnections
                .Where(uc => uc.UserId == userId && uc.HubType == hubType)
                .Select(uc => uc.ConnectionId)
                .ToListAsync();
        }
    }
}
