
namespace Shared.DataTransferObject.Calendar
{
    public class TechnicianCalendarDto
    {
        public int ReservationId { get; set; }

        public string? ClientName { get; set; }
        public string? ClientImage { get; set; }

        public string? Description { get; set; }

        public string? TechTimeInterval { get; set; }

        public DateOnly? Day { get; set; }

        public List<string>? ServiceImages { get; set; }

        public string? Governorate { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }

        public string? SpecialSign { get; set; }
    }
}
