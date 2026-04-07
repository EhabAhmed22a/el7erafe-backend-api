using DomainLayer.Models.IdentityModule;
using Shared.DataTransferObject.ClientDTOs;
using Shared.DataTransferObject.ClientIdentityDTOs;
using Shared.DataTransferObject.OffersDTOs;
using Shared.DataTransferObject.OtpDTOs;
using Shared.DataTransferObject.ReservationDTOs;
using Shared.DataTransferObject.ServiceRequestDTOs;
using Shared.DataTransferObject.UpdateDTOs;

namespace ServiceAbstraction
{
    public interface IClientService
    {
        Task<ServiceListDto> GetClientServicesAsync();
        Task<BroadCastServiceRequestDTO> ServiceRequest(ServiceRequestRegDTO requestRegDTO, string userId);
        Task<List<ServiceRequestDTO>> GetPendingServiceRequestsAsync(string userId);
        Task DeleteAccount(string userId);
        Task<ClientProfileDTO> GetProfileAsync(string userId);
        Task<List<OfferResultDto>> GetOffersAsync(string userId, int requestId, bool isQuick);
        Task<List<CurrentReservationsDTO>> GetCurrentReservationsAsync(string userId);
        Task<List<AvailableTechnicianDto>> GetAvailableTechniciansAsync(GetAvailableTechniciansRequest requestRegDTO);
        Task UpdateNameAndImage(string userId, UpdateNameImageDTO dTO);
        Task UpdatePhoneNumber(string userId, UpdatePhoneDTO dTO);
        Task<OtpResponseDTO> UpdatePendingEmail(string userId, UpdateEmailDTO updateEmailDTO);
        Task UpdateEmailAsync(string userId, OtpCodeDTO otpCode);
        Task<string?> CancelRequestAsync(string userId, ReqIdDTO reqDTO);
        Task<OtpResponseDTO> ResendOtpForPendingEmail(string userId);
        Task<Client?> GetClientByIdAsync(int clientId);
        Task<AcceptOfferResultDto> AcceptOffer(int offerId);
        Task<DeclineOfferResultDto> DeclineOffer(int offerId);
        Task<List<PreviousReservationsDTO>> GetPreviousReservations(string userId);
    }
}
