using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface ITechnicianServicesRepository
    {
        Task<IEnumerable<TechnicianService>?> GetAllAsync();
        Task<IEnumerable<TechnicianService>?> GetPagedAsync(int pageNumber, int pageSize);
        Task<TechnicianService> CreateAsync(TechnicianService technicianService);
        Task<bool> ExistsAsync(string serviceName);
        Task<bool> ExistsAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateAsync(TechnicianService technicianService);
        Task<TechnicianService?> GetByIdAsync(int id);
        Task<TechnicianService?> GetServiceByNameAsync(string serviceName);
    }
}
