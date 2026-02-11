
using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IServiceRequestRepository
    {
        Task<ServiceRequest> GetServiceById(int id);
        Task<bool> IsServiceAlreadyReq(int clientId);
        Task<bool> IsAlwaysAvailable(int clientId);
        Task<TimeOnly?> GetServiceTime(int clientId);
    }
}
