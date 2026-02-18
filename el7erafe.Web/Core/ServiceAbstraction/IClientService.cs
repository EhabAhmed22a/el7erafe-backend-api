
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace ServiceAbstraction
{
    public interface IClientService
    {
        Task<ServiceListDto> GetClientServicesAsync();
        Task QuickReserve(ServiceRequestRegDTO requestRegDTO, string userId);
        Task DeleteAccount(string userId);
        Task<ClientProfileDTO> GetProfileAsync(string userId);
        Task UpdateNameAndImage(string userId, UpdateNameImageDTO dTO);
        Task UpdatePhoneNumber(string userId, UpdatePhoneDTO dTO);
    }
}
