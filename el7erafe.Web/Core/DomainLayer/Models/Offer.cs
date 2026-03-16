
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models
{
    public class Offer
    {
        public int Id { get; set; }
        public decimal Fees { get; set; }
        public DateTime SentAt { get; set; }
        public int ServiceRequestId { get; set; }
        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = default!;
        public int? NumberOfDays { get; set; }
        public TimeOnly? WorkFrom { get; set; }
        public TimeOnly? WorkTo { get; set; }

        public ServiceRequest ServiceRequest { get; set; } = default!;
        public Reservation Reservation { get; set; } = default!;
    }
}
