
using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IServiceRequestRepository
    {
        Task<ServiceRequest?> GetServiceById(int id);
        Task<bool> UpdateAsync(ServiceRequest serviceRequest);
        Task<bool> IsServicePending(int? clientId, int? serviceId);
        Task<bool> IsTimeConflicted(int clientId, TimeOnly? AvailableFrom, TimeOnly? AvailableTo, DateOnly Date);
        Task<TimeOnly?> GetServiceTime(int clientId);
        Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<int>> GetServiceRequestIdsByClientAsync(int clientId);
        Task<IEnumerable<int>> GetServiceRequestIdsByTechnicianAsync(int techId);
        Task<IEnumerable<ServiceRequest>> GetServiceRequestsByClientAsync(int clientId);
        Task<IEnumerable<ServiceRequest>> GetPendingServiceRequestsByClientAsync(int clientId);
        Task<IEnumerable<ServiceRequest>> GetAvailableServiceRequestsByTechnicianAsync(int techId, int serId, int govId);
    }
}
