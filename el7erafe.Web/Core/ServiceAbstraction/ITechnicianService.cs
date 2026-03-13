
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace ServiceAbstraction
{
    public interface ITechnicianService
    {
        Task<TechnicianProfileDTO> GetProfile(string userId);
        Task UpdateBasicInfo(string userId, UpdateTechnicianDTO updateTechnicianDTO);
        Task UpdatePhoneNumber (string userId, UpdatePhoneDTO updatePhoneDTO);
        Task<OtpResponseDTO> UpdatePendingEmail(string userId, UpdateEmailDTO updateEmailDTO);
        Task UpdateEmailAsync(string userId, OtpCodeDTO otpCode);
        Task<OtpResponseDTO> ResendOtpForPendingEmail(string userId);
        Task DeleteAccount(string userId);
    }
}
