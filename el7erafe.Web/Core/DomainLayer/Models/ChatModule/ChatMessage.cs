using DomainLayer.Models.ChatModule.Enums;
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Models.ChatModule
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = default!;
        public string ReceiverId { get; set; } = default!; 
        public string Message { get; set; } = default!;
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }

        public ApplicationUser Sender { get; set; } = default!;
        public ApplicationUser Receiver { get; set; } = default!;
    }
}
