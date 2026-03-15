
namespace Shared.DataTransferObject.OffersDTOs
{
    public class MakeOfferEventResultDto
    {
        public OfferResultDto? ClientOffer { get; set; } 

        public PendingOfferDto? TechnicianOffer { get; set; }

        public string? ClientUserId { get; set; }
    }
}
