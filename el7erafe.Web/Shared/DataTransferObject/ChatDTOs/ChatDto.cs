
namespace Shared.DataTransferObject.ChatDTOs
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string ReceiverId { get; set; } = default!;
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}
