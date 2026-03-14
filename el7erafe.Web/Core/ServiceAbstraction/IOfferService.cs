
using Shared.DataTransferObject.OffersDTOs;

namespace ServiceAbstraction
{
    public interface IOfferService
    {
        Task<OfferResultDto> MakeOfferAsync(MakeOfferDto dto, string technicianUserId);
    }
}
