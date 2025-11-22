
namespace DomainLayer.Models.IdentityModule
{
    public class Governorate
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = default!;
        public string NameAr { get; set; } = default!;
        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
