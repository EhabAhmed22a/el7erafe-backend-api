using Shared.DataTransferObject.ClientIdentityDTOs;

namespace ServiceAbstraction
{
    public interface IClientAuthenticationService
    {
        Task<ClientDTO> RegisterClientAsync(ClientRegisterDTO clientRegisterDTO);
    }
}
