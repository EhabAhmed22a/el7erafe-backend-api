using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Models.IdentityModule
{
    public class UserToken
    {
        public int Id { get; set; } 
        public string Token { get; set; } = default!;
        public TokenType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
    }
}