using Shared.DataTransferObject.LogoutDTOs;

namespace ServiceAbstraction
{
    public interface ILOgoutService
    {
        Task<LogoutResponseDto> LogoutAsync(string token);
    }
}
