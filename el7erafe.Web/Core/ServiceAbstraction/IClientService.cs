
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;

namespace ServiceAbstraction
{
    public interface IClientService
    {
        Task<ServiceListDto> GetClientServicesAsync();
        Task<ServiceReqDTO> QuickReserve(ServiceRequestRegDTO requestRegDTO, string userId);
    }
}
