using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ChatDTOs
{
    public class SendMessageDto
    {
        [Required]
        public string ReceiverId { get; set; } = default!;

        [Required]
        [MaxLength(4000)]              
        public string Content { get; set; } = default!;
    }
}
