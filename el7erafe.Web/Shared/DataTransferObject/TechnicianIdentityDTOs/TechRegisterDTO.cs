using Microsoft.AspNetCore.Http;
using Shared.Validations;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechRegisterDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "Please enter a valid Egyptian phone number (11 digits starting with 010, 011, 012, or 015)")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be exactly 11 digits")]
        public string PhoneNumber { get; set; } = default!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = default!;

        [Required(ErrorMessage = "National ID Front image is required")]
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile NationalIdFront { get; set; } = default!;

        [Required(ErrorMessage = "National ID Back image is required")]
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile NationalIdBack { get; set; } = default!;

        [Required(ErrorMessage = "Criminal Record image is required")]
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile CriminalRecord { get; set; } = default!;

        [Required(ErrorMessage = "Service type is required")]
        public string ServiceType { get; set; } = default!;

        [Required(ErrorMessage = "Governorate is required")]
        public string Governorate { get; set; } = default!; 

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = default!;

    }
}
