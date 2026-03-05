
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models.ChatModule
{
    public class Chat
    {
        public int Id { get; set; }
        public int ClientId { get; set; } 
        public int TechnicianId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Client Client { get; set; } = default!;
        public Technician Technician { get; set; } = default!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
