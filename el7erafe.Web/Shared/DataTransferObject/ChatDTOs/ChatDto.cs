
namespace Shared.DataTransferObject.ChatDTOs
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = default!;
        public string TechnicianId { get; set; } = default!;
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}
