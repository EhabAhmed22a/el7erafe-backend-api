
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace ServiceAbstraction
{
    public interface ITechnicianService
    {
        Task<TechnicianProfileDTO> GetProfile(string userId);
        Task UpdateBasicInfo(string userId, UpdateTechnicianDTO updateTechnicianDTO);
    }
}
