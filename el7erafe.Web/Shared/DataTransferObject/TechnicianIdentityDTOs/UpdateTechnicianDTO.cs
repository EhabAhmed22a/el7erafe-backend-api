
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Shared.Validations;

namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class UpdateTechnicianDTO
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "الاسم يجب أن يكون بين ٢ و ١٠٠ حرف")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "الاسم يمكن أن يحتوي فقط على أحرف ومسافات")]
        public string? Name { get; set; }
        [StringLength(200, ErrorMessage = "نبذة عني لا يمكن أن تتجاوز ٢٠٠ حرف")]
        public string? AboutMe { get; set; }
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? ProfileImage { get; set; }
        [ValidateFileList(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" }, 20)]
        public List<IFormFile> NewPortifolioImages { get; set; } = new List<IFormFile>();
        public List<string> DeletedPortifolioImages { get; set; } = new List<string>();
    }
}
