
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.LoginDTOs
{
    public class ForgetPasswordDTO
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
        public string Email { get; set; } = null!;
    }
}
