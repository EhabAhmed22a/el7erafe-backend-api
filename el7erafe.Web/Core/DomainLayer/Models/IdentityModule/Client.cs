
namespace DomainLayer.Models.IdentityModule
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string UserId { get; set; } = null!; //FK
        public ApplicationUser User { get; set; } = null!;

    }
}
