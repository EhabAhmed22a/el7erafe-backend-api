
using Shared.DataTransferObject.AdminDTOs.Dashboard;
using Shared.DataTransferObject.LoginDTOs;

namespace ServiceAbstraction
{
    public interface IAdminDashboardService
    {
        Task<ClientListDTO> GetClientsAsync(int? pageNumber, int? pageSize);
    }
}
