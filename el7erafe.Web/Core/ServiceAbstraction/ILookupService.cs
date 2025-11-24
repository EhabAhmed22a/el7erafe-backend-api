
using Shared.DataTransferObject.LookupDTOs;

namespace ServiceAbstraction
{
    public interface ILookupService
    {
        Task<ServicesDto> GetAllServicesAsync();
    }
}
