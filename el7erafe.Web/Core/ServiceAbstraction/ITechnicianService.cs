
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace ServiceAbstraction
{
    public interface ITechnicianService
    {
        Task<TechnicianProfileDTO> GetProfile(string userId);
        Task UpdateBasicInfo(string userId, UpdateTechnicianDTO updateTechnicianDTO);
        Task UpdatePhoneNumber (string userId, UpdatePhoneDTO updatePhoneDTO);
    }
}
