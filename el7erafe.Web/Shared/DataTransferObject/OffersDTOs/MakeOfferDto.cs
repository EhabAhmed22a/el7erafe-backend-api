
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.OffersDTOs
{
    public class MakeOfferDto
    {
        public int RequestId { get; set; }

        public decimal Fees { get; set; }

        public TimeOnly FromTime { get; set; }

        public TimeOnly ToTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "عدد الأيام يجب أن يكون أكبر من صفر.")]
        public int? NumberOfDays { get; set; }
    }
}
