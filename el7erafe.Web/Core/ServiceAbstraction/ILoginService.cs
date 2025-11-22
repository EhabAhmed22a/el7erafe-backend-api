using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObject.LoginDTOs;

namespace ServiceAbstraction
{
    public interface ILoginService
    {
        Task<UserDTO> LoginAsync(LoginDTO loginDTO);
    }
}
