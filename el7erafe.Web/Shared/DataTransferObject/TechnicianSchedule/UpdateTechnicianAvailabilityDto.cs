
namespace Shared.DataTransferObject.TechnicianSchedule
{
    public class UpdateTechnicianAvailabilityDto
    {
        public int Id { get; set; }

        public int? DayOfWeek { get; set; }

        public TimeOnly FromTime { get; set; }

        public TimeOnly ToTime { get; set; }
    }
}
