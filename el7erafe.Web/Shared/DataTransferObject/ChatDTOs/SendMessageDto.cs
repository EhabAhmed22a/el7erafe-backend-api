using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ChatDTOs
{
    public class SendMessageDto
    {
        [Required]
        public int ChatId { get; set; } 

        [Required]
        public string MessageType { get; set; } = "Text";

        [Required]
        [MaxLength(4000)]              
        public string Content { get; set; } = default!;
    }
}
