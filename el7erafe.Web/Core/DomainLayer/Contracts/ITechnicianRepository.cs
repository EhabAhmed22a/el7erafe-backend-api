using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Contracts
{
    public interface ITechnicianRepository
    {
        Task<Technician> CreateAsync(Technician technician); //Create technician 
        Task<Technician?> GetByIdAsync(int id); //Get technician by id
        Task<bool> EmailExistsAsync(string email);
        Task<Technician?> GetByUserIdAsync(string userId); //Get technician by userId
        Task<Technician?> GetFullTechnicianByUserIdAsync(string userId); //Get full technician details by userId
        Task<IEnumerable<Technician>?> GetAllAsync(); //Get all technicians except the pending ones
        Task<IEnumerable<Technician>?> GetAllByStatusAsync(TechnicianStatus status); //Get all technicians by status
        Task<int> UpdateAsync(Technician technician); //Update technician
        Task<int> DeleteAsync(string id); //Delete technician
        Task<bool> ExistsAsync(int id); //Check if technician exists by id
        Task<bool> ExistsAsync(string phoneNumber); //Check if technician exists by phone number
        Task<IEnumerable<TechnicianService>?> GetAllServicesAsync(); //Get all services
        Task<Governorate?> GetGovernorateByNameAsync(string nameAr);
        Task<City?> GetCityByNameAsync(string nameAr, int governorateId);
        Task<TechnicianService?> GetServiceByNameAsync(string nameAr);
        Task<IEnumerable<Technician>?> GetPagedAsync(int pageNumber, int pageSize); //Get all technicians except the pending ones
        Task<IEnumerable<Technician>?> GetPagedByStatusAsync(TechnicianStatus status, int pageNumber, int pageSize);
        Task<IEnumerable<Technician>?> GetTechniciansByGovernorateWithCityPriorityAsync(int governorateId, int preferredCityId,bool sorted);
    }
}
