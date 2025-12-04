using DomainLayer.Models.IdentityModule.Enums;
using Shared.DataTransferObject.AdminDTOs.Dashboard;

namespace ServiceAbstraction
{
    public interface IAdminDashboardService
    {
        Task<ClientListDTO> GetClientsAsync(int? pageNumber, int? pageSize);
        Task<ServiceListDTO> GetServicesAsync(int? pageNumber, int? pageSize);
        Task<ServiceDTO> CreateServiceAsync(ServiceRegisterDTO serviceRegisterDTO);
        Task DeleteServiceAsync(int id);
        Task DeleteClientAsync(string userId);
        Task UpdateServiceAsync(int id, ServiceUpdateDTO serviceUpdateDTO);
        Task BlockUnblockClientAsync(BlockUnblockDTO blockDTO, string userId);
        Task<TechnicianListDTO> GetTechniciansAsync(int? pageNumber, int? pageSize);
        Task DeleteTechnicianAsync(string userId);
        Task<RejectionCommentsResponseDTO> GetRejectionCommentsAsync();
        Task<TechnicianListDTO> GetTechnicianRequestsAsync(int? pageNumber, int? pageSize, TechnicianStatus technicianStatus);
        Task ApproveTechnicianAsync(string userId);
        Task RejectTechnicianAsync(RejectTechDTO rejectTechDTO);
        Task BlockUnblockTechnicianAsync(BlockUnblockDTO blockDTO, string userId);
    }
}
