
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.UpdateDTOs
{
    public class UpdateEmailDTO
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
        public string NewEmail { get; set; } = default!;
    }
}
