
namespace DomainLayer.Models.IdentityModule
{
    public class TechnicianService
    {
        public int Id { get; set; } 
        public string NameAr { get; set; } = default!;
        public string? ServiceImageURL { get; set; } = default!;
        public ICollection<Technician> Technicians { get; set; } = new List<Technician>();
    }
}
