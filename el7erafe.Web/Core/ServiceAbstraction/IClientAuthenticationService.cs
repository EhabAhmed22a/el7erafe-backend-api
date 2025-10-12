using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace ServiceAbstraction
{
    public interface IClientAuthenticationService
    {
        Task<OtpResponseDTO> RegisterAndSendOtpAsync(ClientRegisterDTO clientRegisterDTO);
        Task<ClientDTO> VerifyOtpAndCompleteRegistrationAsync(OtpVerificationDTO otpVerificationDTO);
    }
}
