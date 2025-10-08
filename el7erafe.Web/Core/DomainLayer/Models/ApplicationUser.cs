
using Microsoft.AspNetCore.Identity;

namespace DomainLayer.Models
{
    public class ApplicationUser: IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
