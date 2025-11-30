
namespace DomainLayer.Models.IdentityModule
{
    public class Admin
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ApplicationUser User { get; set; } = default!;
        public string UserId { get; set; } = default!;
    }
}
