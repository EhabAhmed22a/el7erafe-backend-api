using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Models.IdentityModule
{
    public class TechnicianAvailability
    {
        public int Id { get; set; }
        public string TechnicianId { get; set; } = default!;
        public WeekDay? DayOfWeek { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }

        public Technician Technician { get; set; } = default!;
    }
}
