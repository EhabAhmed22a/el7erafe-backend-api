
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.OtpDTOs
{
    public class ResendOtpRequestDTO
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
        public string Email { get; set; } = null!;
    }
}
