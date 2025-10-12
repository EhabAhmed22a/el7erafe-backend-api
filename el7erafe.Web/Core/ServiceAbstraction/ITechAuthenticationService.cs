using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace ServiceAbstraction
{
    public interface ITechAuthenticationService
    {
        Task<TechDTO> techRegisterAsync(TechRegisterDTO techRegisterDTO);

    }
}
