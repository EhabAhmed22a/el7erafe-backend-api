
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface IRejectionRepository
    {
        Task<Rejection> GetByIdAsync(int id);
        Task AddAsync(Rejection rejection);
        Task UpdateAsync(Rejection rejection);
        Task DeleteAsync(int id);
        Task<bool> ExistsForTechnicianAsync(int technicianId);
    }
}
