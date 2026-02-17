
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;

namespace ServiceAbstraction
{
    public interface IClientService
    {
        Task<ServiceListDto> GetClientServicesAsync();
        Task QuickReserve(ServiceRequestRegDTO requestRegDTO, string userId);
        Task<ClientProfileDTO> GetProfileAsync(string userId);
    }
}
