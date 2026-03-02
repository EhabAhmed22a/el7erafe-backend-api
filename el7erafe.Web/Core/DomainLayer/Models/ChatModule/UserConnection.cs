using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models.ChatModule
{
    public class UserConnection
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public string ConnectionId { get; set; } = default!;
        public DateTime ConnectedAt { get; set; }

        public ApplicationUser User { get; set; } = default!;
    }
}
