using Shared.DataTransferObject.ClientIdentityDTOs;

namespace ServiceAbstraction
{
    public interface IClientAuthenticationService
    {
        Task<ClientDTO> RegisterClientAsync(ClientRegisterDTO client);
        Task<ClientDTO> LoginClientAsync(ClientLoginDTO client);
        Task LogoutAsync();
    }
}
