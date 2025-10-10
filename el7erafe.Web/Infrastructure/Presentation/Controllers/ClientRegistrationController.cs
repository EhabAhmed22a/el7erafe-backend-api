using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;
using Shared.DataTransferObject.ClientIdentityDTOs;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class ClientRegistrationController(IClientAuthenticationService service
        , ILogger<ClientRegistrationController> logger) : ControllerBase
    {
        [HttpPost("register/client")]
        public async Task<ActionResult<ClientDTO>> Register(ClientRegisterDTO clientRegisterDTO)
        {
            logger.LogInformation("[API] Registering client with phone: {Phone}", clientRegisterDTO.PhoneNumber);

            var client = await service.RegisterClientAsync(clientRegisterDTO);

            logger.LogInformation("[API] Client registered successfully with Name: {ClientName}", client.Name);
            return Ok(client);
        }
    }
}