using System.ComponentModel.DataAnnotations;

namespace DomainLayer.Models.IdentityModule
{
    public class Rejection
    {
        public int Id { get; set; }

        [StringLength(500)]
        public string Reason { get; set; } = default!;
        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = default!;

    }
}