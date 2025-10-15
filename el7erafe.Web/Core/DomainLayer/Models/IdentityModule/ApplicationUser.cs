using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.AspNetCore.Identity;

namespace DomainLayer.Models.IdentityModule
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Technician? Technician { get; set; } 
        public Client? Client { get; set; }
        public UserTypeEnum UserType { get; set; } 
    }
}
