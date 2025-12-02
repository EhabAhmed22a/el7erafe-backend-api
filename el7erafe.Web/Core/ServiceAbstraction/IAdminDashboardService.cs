
using DomainLayer.Models.IdentityModule;
using Shared.DataTransferObject.AdminDTOs.Dashboard;
using Shared.DataTransferObject.LoginDTOs;

namespace ServiceAbstraction
{
    public interface IAdminDashboardService
    {
        Task<ClientListDTO> GetClientsAsync(int? pageNumber, int? pageSize);
        Task<ServiceListDTO> GetServicesAsync(int? pageNumber, int? pageSize);
        Task<ServiceDTO> CreateServiceAsync(ServiceRegisterDTO serviceRegisterDTO);
        Task DeleteServiceAsync(int id);
        Task DeleteClientAsync(string userId);
    }
}
