using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Contracts
{
    public interface ITechnicianRepository
    {
        Task<Technician> CreateAsync(Technician technician);
        Task<Technician?> GetByIdAsync(int id);
        Task<Technician?> GetByUserIdAsync(string userId);
        Task<IEnumerable<Technician>> GetAllAsync();
        Task<IEnumerable<Technician>> GetAllByStatusAsync(TechnicianStatus status);
        Task UpdateAsync(Technician technician);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNationalIdAsync(string nationalId);
    }
}
