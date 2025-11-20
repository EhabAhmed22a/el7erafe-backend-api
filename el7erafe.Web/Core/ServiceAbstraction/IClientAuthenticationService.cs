using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace ServiceAbstraction
{
    public interface IClientAuthenticationService
    {
        Task<OtpResponseDTO> RegisterAndSendOtpAsync(ClientRegisterDTO clientRegisterDTO);
        Task<OtpResponseDTO> ResendOtp(ResendOtpRequestDTO resendOtpRequestDTO);
        Task<ClientDTO> VerifyOtpAndCompleteRegistrationAsync(OtpVerificationDTO otpVerificationDTO);
    }
}
