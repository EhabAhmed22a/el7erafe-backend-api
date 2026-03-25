
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.OffersDTOs
{
    public class MakeOfferDto
    {
        [Required(ErrorMessage = "رقم الطلب مطلوب.")]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "قيمة العرض مطلوبة.")]
        [Range(1, double.MaxValue, ErrorMessage = "قيمة العرض يجب أن تكون أكبر من صفر.")]
        public decimal Fees { get; set; }

        [Required(ErrorMessage = "وقت البداية مطلوب.")]
        public TimeOnly FromTime { get; set; }

        [Required(ErrorMessage = "وقت النهاية مطلوب.")]
        public TimeOnly ToTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "عدد الأيام يجب أن يكون أكبر من صفر.")]
        public int? NumberOfDays { get; set; }
    }
}
