
namespace Shared.DataTransferObject.OffersDTOs
{
    public class DeclineOfferResultDto
    {
        public int RequestId { get; set; }

        public int OfferId { get; set; }

        public string TechnicianUserId { get; set; } = default!;
    }
}
