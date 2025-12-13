using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.TechnicianIdentityDTOs;

namespace ServiceAbstraction
{
    public interface ITechAuthenticationService
    {
        Task<OtpResponseDTO> techRegisterAsync(TechRegisterDTO techRegisterDTO);
        Task<UserDTO> ConfirmEmailAsync(OtpVerificationDTO otpVerificationDTO);
        Task<UserDTO> CheckTechnicianApprovalAsync(string userId);
        Task<TechResubmitResponseDTO> TechnicianResubmitDocumentsAsync(TechResubmitDTO techResubmitDTO);
    }
}
