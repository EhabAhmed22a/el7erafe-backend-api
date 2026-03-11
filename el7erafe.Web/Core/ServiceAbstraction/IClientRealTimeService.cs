
using DomainLayer.Models.ChatModule;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Shared.DataTransferObject.ServiceRequestDTOs;

namespace ServiceAbstraction
{
    public interface IClientRealTimeService
    {
        Task<UserConnection> AddUserConnectionAsync(string userId, string connectionId);
        Task RemoveConnectionAsync(string userId);
        Task<List<ServiceRequestDTO>> GetServiceRequestsAsync(string userId);
    }
}
