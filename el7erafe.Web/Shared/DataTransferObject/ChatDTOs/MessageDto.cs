
namespace Shared.DataTransferObject.ChatDTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string SenderId { get; set; } = default!;
        public string ReceiverId { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
