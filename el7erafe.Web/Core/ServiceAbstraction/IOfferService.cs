
using Shared.DataTransferObject.OffersDTOs;

namespace ServiceAbstraction
{
    public interface IOfferService
    {
        Task<MakeOfferEventResultDto> MakeOfferAsync(MakeOfferDto dto, string technicianUserId);
    }
}
