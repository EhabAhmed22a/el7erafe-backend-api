
using DomainLayer.Models.ChatModule;
using DomainLayer.Models.ChatModule.Enums;

namespace DomainLayer.Contracts.ChatModule
{
    public interface IUserConnectionRepository
    {
        // Add a new connection
        Task<UserConnection> AddConnectionAsync(string userId, string connectionId, HubType hubType);

        // Remove a connection (when user disconnects)
        Task RemoveConnectionAsync(string connectionId);

        // Get all active connections for a user
        Task<List<string>> GetUserConnectionsAsync(string userId);

        // Check if user has any active connections
        Task<bool> IsUserOnlineAsync(string userId, HubType hubType);

        // Get connection details
        Task<UserConnection?> GetConnectionByIdAsync(string connectionId);

        Task<List<string>> GetUserConnectionsByTypeAsync(string userId, HubType hubType);
    }
}
