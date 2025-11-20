
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ClientIdentityDTOs
{
    public class ClientRegisterDTO
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "الاسم يجب أن يكون بين ٢ و ١٠٠ حرف")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "الاسم يمكن أن يحتوي فقط على أحرف ومسافات")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "يرجى إدخال رقم هاتف مصري صحيح (١١ رقماً يبدأ ب ٠١٠، ٠١١، ٠١٢، أو ٠١٥)")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "رقم الهاتف يجب أن يكون ١١ رقماً بالضبط")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = null!;
    }
}
