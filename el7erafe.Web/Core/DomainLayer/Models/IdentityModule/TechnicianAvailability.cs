using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Models.IdentityModule
{
    public class TechnicianAvailability
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; } 
        public WeekDay? DayOfWeek { get; set; }
        public TimeOnly FromTime { get; set; }
        public TimeOnly ToTime { get; set; }

        public Technician Technician { get; set; } = default!;
    }
}
