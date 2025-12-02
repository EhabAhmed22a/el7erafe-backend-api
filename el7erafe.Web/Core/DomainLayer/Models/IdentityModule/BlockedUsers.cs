
namespace DomainLayer.Models.IdentityModule
{
    public class BlockedUser
    {
        public int Id { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
