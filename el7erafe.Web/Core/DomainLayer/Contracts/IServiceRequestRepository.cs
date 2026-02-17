
using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IServiceRequestRepository
    {
        Task<ServiceRequest?> GetServiceById(int id);
        Task<bool> UpdateAsync(ServiceRequest serviceRequest);
        Task<bool> IsServiceAlreadyReq(int? clientId, int? serviceId);
        Task<bool> IsTimeConflicted(int clientId, TimeOnly? AvailableFrom, TimeOnly? AvailableTo, DateOnly Date);
        Task<TimeOnly?> GetServiceTime(int clientId);
        Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest);
        Task<int> DeleteAsync(int id);
        Task<IEnumerable<int>> GetServiceRequestIdsByClientAsync(int clientId);
    }
}
