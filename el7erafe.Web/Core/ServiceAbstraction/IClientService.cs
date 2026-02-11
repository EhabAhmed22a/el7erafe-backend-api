
using Shared.DataTransferObject.ClientDTOs;

namespace ServiceAbstraction
{
    public interface IClientService
    {
        Task<ServiceListDto> GetClientServicesAsync();
    }
}
