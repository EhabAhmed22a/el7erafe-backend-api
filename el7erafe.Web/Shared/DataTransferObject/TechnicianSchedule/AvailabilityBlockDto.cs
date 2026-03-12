using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.TechnicianSchedule
{
    public class AvailabilityBlockDto
    {
        [Range(1, 7, ErrorMessage = "يجب أن يكون رقم اليوم بين 1 و 7 أو تركه فارغًا للتوفر في جميع الأيام")]
        public int? DayOfWeek { get; set; }

        [Required(ErrorMessage = "وقت البداية مطلوب")]
        public TimeOnly FromTime { get; set; }

        [Required(ErrorMessage = "وقت النهاية مطلوب")]
        public TimeOnly ToTime { get; set; }
    }
}
