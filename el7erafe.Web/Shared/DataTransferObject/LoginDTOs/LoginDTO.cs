using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.LoginDTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "يرجى إدخال رقم هاتف مصري صحيح (١١ رقماً يبدأ ب ٠١٠، ٠١١، ٠١٢، أو ٠١٥)")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "رقم الهاتف يجب أن يكون ١١ رقماً بالضبط")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = null!;
    }
}
