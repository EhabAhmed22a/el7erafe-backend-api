using DomainLayer.Models.ChatModule.Enums;
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models.ChatModule
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string SenderId { get; set; } = default!;
        public string ReceiverId { get; set; } = default!; 
        public string Content { get; set; } = default!;
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageStatus Status { get; set; }
        public bool IsDeleted { get; set; }

        public Chat Chat { get; set; } = default!;
        public ApplicationUser Sender { get; set; } = default!;
        public ApplicationUser Receiver { get; set; } = default!;
    }
}
