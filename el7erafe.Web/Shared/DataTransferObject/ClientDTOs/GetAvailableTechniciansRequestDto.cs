
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ClientDTOs
{
    public class GetAvailableTechniciansRequest
    {
        [Required(ErrorMessage = "الخدمة مطلوبة")]
        public string ServiceName { get; set; } = default!;

        [Required(ErrorMessage = "اسم المدينة مطلوب")]
        public string CityName { get; set; } = default!;

        //[Required(ErrorMessage = "يجب تحديد يوم الأسبوع")]
        //[Range(1, 7, ErrorMessage = "يجب أن يكون رقم اليوم بين 1 و 7")]
        //public int DayOfWeek { get; set; }

        //[Required(ErrorMessage = "وقت البداية مطلوب")]
        //public TimeOnly FromTime { get; set; }

        //[Required(ErrorMessage = "وقت النهاية مطلوب")]
        //public TimeOnly ToTime { get; set; }

        public bool Sorted { get; set; }
    }

}
