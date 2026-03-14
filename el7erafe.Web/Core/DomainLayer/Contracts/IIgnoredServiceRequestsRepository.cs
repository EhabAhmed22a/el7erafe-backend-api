
using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IIgnoredServiceRequestsRepository
    {
        Task<IgnoredServiceRequest> CreateAsync(IgnoredServiceRequest request);
        Task<bool> DeleteAllByTechnicianId(int techId);
        Task<bool> IsAlreadyIgnoredAsync(int techId, int serviceId);
    }
}
