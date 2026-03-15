
using DomainLayer.Models.IdentityModule;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace ServiceAbstraction
{
    public interface ITechnicianFlowService
    {
        Task<TechnicianProfileDTO> GetProfile(string userId);
        Task UpdateBasicInfo(string userId, UpdateTechnicianDTO updateTechnicianDTO);
        Task UpdatePhoneNumber (string userId, UpdatePhoneDTO updatePhoneDTO);
        Task<OtpResponseDTO> UpdatePendingEmail(string userId, UpdateEmailDTO updateEmailDTO);
        Task<Technician?> GetTechnicianByIdAsync(int techId);
        Task<string?> DeclineRequestAsync(string userId, ReqIdDTO cancelReqDTO);
        Task<List<BroadCastServiceRequestDTO>> GetAvailableRequests(string userId);
        Task UpdateEmailAsync(string userId, OtpCodeDTO otpCode);
        Task<OtpResponseDTO> ResendOtpForPendingEmail(string userId);
        Task DeleteAccount(string userId);
    }
}
