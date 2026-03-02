

using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.OtpDTOs
{
    public class OtpCodeDTO
    {
        [Required(ErrorMessage = "رمز التحقق مطلوب")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "رمز التحقق يجب أن يكون 6 أرقام")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "رمز التحقق يجب أن يحتوي على أرقام فقط")]
        public string OtpCode { get; set; } = default!;
    }
}
