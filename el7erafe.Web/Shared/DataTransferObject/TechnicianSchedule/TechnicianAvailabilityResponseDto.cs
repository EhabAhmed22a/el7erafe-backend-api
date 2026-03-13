
namespace Shared.DataTransferObject.TechnicianSchedule
{
    public class TechnicianAvailabilityResponseDto
    {
        public int? DayOfWeek { get; set; }

        public TimeOnly FromTime { get; set; }

        public TimeOnly ToTime { get; set; }
    }
}
