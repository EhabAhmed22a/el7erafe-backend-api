
namespace Shared.DataTransferObject.OffersDTOs
{
    public class MakeOfferDto
    {
        public int RequestId { get; set; }

        public decimal Fees { get; set; }

        public TimeOnly FromTime { get; set; }

        public TimeOnly ToTime { get; set; }

        public int? NumberOfDays { get; set; }
    }
}
