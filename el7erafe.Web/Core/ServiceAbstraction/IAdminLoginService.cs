
using Shared.DataTransferObject.AdminDTOs.LoginDTO;

namespace ServiceAbstraction
{
    public interface IAdminLoginService
    {
        Task<AdminDTO> LoginAsync(AdminLoginDTO adminLoginDTO);
    }
}
