
namespace Shared.DataTransferObject.OffersDTOs
{
    public class AcceptOfferResultDto
    {
        public int RequestId { get; set; }

        public int AcceptedOfferId { get; set; }

        public List<string> TechnicianUserIds { get; set; } = new();
    }
}
