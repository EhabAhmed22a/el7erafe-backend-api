using DomainLayer.Models.ChatModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;

namespace DomainLayer.Models.IdentityModule
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Technician? Technician { get; set; } 
        public Client? Client { get; set; }
        public Admin? Admin { get; set; }
        public UserTypeEnum UserType { get; set; }
        public virtual UserToken? UserToken { get; set; }
        public BlockedUser? BlockedUser { get; set; }
        public string? PendingEmail{ get; set; }
        public ICollection<UserConnection> UserConnections { get; set; } = new List<UserConnection>();
        public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
        public ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();
    }
}
