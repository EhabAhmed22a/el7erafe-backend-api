
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = default!;

        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;

        public string Action { get; set; } = default!;        
        public string? ExtraPayload { get; set; }         
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = default!;
    }
}