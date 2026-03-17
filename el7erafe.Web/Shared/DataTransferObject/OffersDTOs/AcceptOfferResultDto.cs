
namespace Shared.DataTransferObject.OffersDTOs
{
    public class AcceptOfferResultDto
    {
        public int RequestId { get; set; }

        public int AcceptedOfferId { get; set; }

        public string AcceptedTechnicianUserId { get; set; } = default!;

        public List<string> RejectedTechnicianUserIds { get; set; } = new();
    }
}
