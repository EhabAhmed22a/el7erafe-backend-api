using Microsoft.AspNetCore.Http;
using Shared.Validations;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechResubmitDTO
    {
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string PhoneNumber { get; set; } = default!;
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? NationalIdFront { get; set; }

        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? NationalIdBack { get; set; } 

        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? CriminalRecord { get; set; } 
    }
}
