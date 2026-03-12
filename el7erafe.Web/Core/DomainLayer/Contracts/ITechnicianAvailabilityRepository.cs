using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface ITechnicianAvailabilityRepository
    {
        Task<TechnicianAvailability> CreateAsync(TechnicianAvailability availability);
        Task<TechnicianAvailability?> GetByIdAsync(int id);
        Task<IEnumerable<TechnicianAvailability>?> GetByTechnicianIdAsync(int technicianId);
        Task<int> UpdateAsync(TechnicianAvailability availability);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsForTechnicianAsync(int technicianId);
        Task<int> DeleteAsync(int id);
        Task<int> DeleteByTechnicianIdAsync(int technicianId);
    }
}
