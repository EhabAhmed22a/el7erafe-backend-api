
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models.ChatModule
{
    public class Chat
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = default!;
        public string TechnicianId { get; set; } = default!;

        public ApplicationUser Client { get; set; } = default!;
        public ApplicationUser Technician { get; set; } = default!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
