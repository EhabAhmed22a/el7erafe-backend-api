using System.Threading.Tasks;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Contracts
{
    public interface ITechnicianAvailabilityRepository
    {
        Task<TechnicianAvailability> CreateAsync(TechnicianAvailability availability);
        Task CreateDefaultAvailabilityForTechnicianAsync(int technicianId);
        Task<TechnicianAvailability?> GetByIdAsync(int id);
        Task<IEnumerable<TechnicianAvailability>?> GetByTechnicianIdAsync(int technicianId);
        Task<int> UpdateAsync(TechnicianAvailability availability);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsForTechnicianAsync(int technicianId);
        Task<ICollection<string>> GetAvailableTechsForRequestAsync(int serviceId, int govId, WeekDay date, TimeOnly? from, TimeOnly? to, TimeOnly? minTime);
        Task<List<Technician>> GetCandidateTechsForRequestAsync(
    int serviceId,
    int govId,
    WeekDay date,
    TimeOnly? requestedFrom,
    TimeOnly? requestedTo,
    TimeOnly? minimumStartTime);
        Task<int> DeleteAsync(int id);
        Task<int> DeleteByTechnicianIdAsync(int technicianId);
    }
}
