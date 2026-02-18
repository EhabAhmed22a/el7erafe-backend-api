
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace ServiceAbstraction
{
    public interface IClientService
    {
        Task<ServiceListDto> GetClientServicesAsync();
        Task ServiceRequest(ServiceRequestRegDTO requestRegDTO, string userId);
        Task DeleteAccount(string userId);
        Task<ClientProfileDTO> GetProfileAsync(string userId);
        Task<List<AvailableTechnicianDto>> GetAvailableTechniciansAsync(GetAvailableTechniciansRequest requestRegDTO);
        Task UpdateNameAndImage(string userId, UpdateNameImageDTO dTO);
        Task UpdatePhoneNumber(string userId, UpdatePhoneDTO dTO);
        Task<OtpResponseDTO> UpdateEmail(string userId, UpdateEmailDTO updateEmailDTO);
    }
}
