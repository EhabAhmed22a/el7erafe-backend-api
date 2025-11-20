
namespace DomainLayer.Models.IdentityModule
{
    public class City
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = default!;
        public string NameAr { get; set; } = default!;
        public int GovernorateId { get; set; }
        public Governorate Governorate { get; set; } = default!;
        public ICollection<Technician> Technicians { get; set; } = new List<Technician>();
    }
}
