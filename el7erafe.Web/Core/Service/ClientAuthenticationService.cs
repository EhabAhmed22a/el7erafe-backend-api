
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;

namespace Service
{
    public class ClientAuthenticationService() : IClientAuthenticationService
    {
        public Task<ClientDTO> RegisterClientAsync(ClientRegisterDTO client)
        {
            
        }
        public Task<ClientDTO> LoginClientAsync(ClientLoginDTO client)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }
    }
}
