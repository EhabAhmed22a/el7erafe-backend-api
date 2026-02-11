
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public string Description { get; set; } = default!;
        public City City { get; set; } = default!;
        public int CityId { get; set; }
        public TechnicianService Service { get; set; } = default!;
        public int ServiceId { get; set; }
        public string Street { get; set; } = default!;
        public string? SpecialSign { get; set; } = default!;
        public TimeOnly? AvailableFrom { get; set; }
        public TimeOnly? AvailableTo { get; set; }
        public DateOnly ServiceDate { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = default!;
        public Technician? Technician { get; set; }
        public int? TechnicianId { get; set; }
        public string? LastImageURL { get; set; }
    }
}
