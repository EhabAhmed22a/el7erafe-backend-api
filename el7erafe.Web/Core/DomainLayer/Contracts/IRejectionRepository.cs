
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface IRejectionRepository
    {
        Task<Rejection> GetByIdAsync(int id);
        Task<int> CreateAsync(Rejection rejection);
        Task<int> UpdateAsync(Rejection rejection);
        Task<int> DeleteAsync(int id);
        Task<bool> ExistsForTechnicianAsync(int technicianId);
    }
}
