using Shared.DataTransferObject.LogoutDTOs;

namespace ServiceAbstraction
{
    public interface ILogoutService
    {
        Task<LogoutResponseDto> LogoutAsync(string token);
    }
}
