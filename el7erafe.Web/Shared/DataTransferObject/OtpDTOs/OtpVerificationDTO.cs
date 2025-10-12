
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.OtpDTOs
{
    public class OtpVerificationDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "OTP must contain only digits")]
        public string OtpCode { get; set; } = null!;
    }
}
