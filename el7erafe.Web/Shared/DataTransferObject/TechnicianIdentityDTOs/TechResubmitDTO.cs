using Microsoft.AspNetCore.Http;
using Shared.Validations;

namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechResubmitDTO
    {
        public string PhoneNumber { get; set; } = default!;
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? NationalIdFront { get; set; }

        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? NationalIdBack { get; set; } 

        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? CriminalRecord { get; set; } 
    }
}
