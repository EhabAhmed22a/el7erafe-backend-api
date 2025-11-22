using Shared.DataTransferObject;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace ServiceAbstraction
{
    public interface ITechAuthenticationService
    {
        Task<TechDTO> techRegisterAsync(TechRegisterDTO techRegisterDTO);
        Task<UserDTO> CheckTechnicianApprovalAsync(string userId);

    }
}
