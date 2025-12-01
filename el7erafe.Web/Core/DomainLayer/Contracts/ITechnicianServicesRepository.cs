using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface ITechnicianServicesRepository
    {
        Task<IEnumerable<TechnicianService>?> GetAllTechnicianServicesAsync();
        Task<IEnumerable<TechnicianService>?> GetPagedTechnicianServicesAsync(int pageNumber, int pageSize);
        Task<TechnicianService> CreateServiceAsync(TechnicianService technicianService);
        Task<bool> ServiceExistsAsync(string serviceName);
    }
}
