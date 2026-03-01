
namespace Shared.DataTransferObject.ChatDTOs
{
    public class InboxConversationDto
    {
        public int ChatId { get; set; }
        public string ReceiverId { get; set; } = default!;
        public string ReceiverName { get; set; } = default!;
        public string? ReceiverImage { get; set; }
        public string? LastMessageContent { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public bool IsLastMessageFromMe { get; set; }
        public int UnreadCount { get; set; }
    }
}