using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ChatDTOs
{
    public class SendMessageDto
    {
        [Required]                      
        public int ChatId { get; set; }

        [Required]
        public string SenderId { get; set; } = default!;

        [Required]
        public string ReceiverId { get; set; } = default!;

        [Required]
        [MaxLength(4000)]              
        public string Content { get; set; } = default!;

        public string Type { get; set; } = "Text"; 
    }
}
