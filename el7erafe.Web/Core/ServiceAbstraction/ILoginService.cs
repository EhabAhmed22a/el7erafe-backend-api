using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObject.LoginDTOs;
using Shared.DataTransferObject.OtpDTOs;

namespace ServiceAbstraction
{
    public interface ILoginService
    {
        Task<UserDTO> LoginAsync(LoginDTO loginDTO);
        Task<OtpResponseDTO> ForgetPasswordAsync(ForgetPasswordDTO forgetPasswordDTO);
    }
}
