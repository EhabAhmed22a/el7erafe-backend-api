
using DomainLayer.Models.ChatModule;

namespace DomainLayer.Contracts.ChatModule
{
    public interface IUserConnectionRepository
    {
        // Add a new connection
        Task<UserConnection> AddConnectionAsync(string userId, string connectionId);

        // Remove a connection (when user disconnects)
        Task RemoveConnectionAsync(string connectionId);

        // Get all active connections for a user
        Task<List<string>> GetUserConnectionsAsync(string userId);

        // Check if user has any active connections
        Task<bool> IsUserOnlineAsync(string userId);

        // Get connection details
        Task<UserConnection?> GetConnectionByIdAsync(string connectionId);
    }
}
