using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace ServiceAbstraction
{
    public interface IClientAuthenticationService
    {
        Task<OtpResponseDTO> RegisterAsync(ClientRegisterDTO clientRegisterDTO);
        Task<OtpResponseDTO> ResendOtp(ResendOtpRequestDTO resendOtpRequestDTO);
        Task<UserDTO> VerifyOtpAsync(OtpVerificationDTO otpVerificationDTO);
    }
}
