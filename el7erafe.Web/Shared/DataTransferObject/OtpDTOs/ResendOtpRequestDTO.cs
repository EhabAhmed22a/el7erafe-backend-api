
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.OtpDTOs
{
    public class ResendOtpRequestDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = null!;
    }
}
