
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ClientIdentityDTOs
{
    public class ClientRegisterDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2,ErrorMessage = "Name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "Please enter a valid Egyptian phone number (11 digits starting with 010, 011, 012, or 015)")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be exactly 11 digits")]
        public string PhoneNumber { get; set; } = null!;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Password is required")]
        public string Password {  get; set; } = null!;
    }
}
