
namespace Shared.DataTransferObject.ChatDTOs
{
    public class ChatDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; } 
        public int TechnicianId { get; set; } 
        public MessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}
