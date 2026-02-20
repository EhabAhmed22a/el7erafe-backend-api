
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Shared.Validations;

namespace Shared.DataTransferObject.UpdateDTOs
{
    public class UpdateNameImageDTO
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "الاسم يجب أن يكون بين ٢ و ١٠٠ حرف")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "الاسم يمكن أن يحتوي فقط على أحرف ومسافات")]
        public string? Name { get; set; }
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile? Image { get; set; }
    }
}
