

using System.ComponentModel.DataAnnotations;
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models
{
    public class IgnoredServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public int TechnicianId { get; set; }

        [Required]
        public int ServiceRequestId { get; set; }

        public DateTime IgnoredAt { get; set; } = DateTime.UtcNow;

        public Technician Technician { get; set; } = default!;
        public ServiceRequest ServiceRequest { get; set; } = default!;
    }
}
