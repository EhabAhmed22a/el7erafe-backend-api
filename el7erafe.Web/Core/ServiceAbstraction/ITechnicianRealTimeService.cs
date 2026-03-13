
using DomainLayer.Models.ChatModule;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace ServiceAbstraction
{
    public interface ITechnicianRealTimeService
    {
        Task<UserConnection> AddUserConnectionAsync(string userId, string connectionId);
        Task RemoveConnectionAsync(string connectionId);
    }
}
