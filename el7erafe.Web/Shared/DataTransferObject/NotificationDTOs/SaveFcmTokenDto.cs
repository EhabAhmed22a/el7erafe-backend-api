
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.NotificationDTOs
{
    public class SaveFcmTokenDto
    {
        [Required]
        public string FcmToken { get; set; } = default!;
    }
}
