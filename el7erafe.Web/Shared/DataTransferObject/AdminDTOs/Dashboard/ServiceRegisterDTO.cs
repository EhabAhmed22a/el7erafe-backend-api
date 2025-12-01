
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Shared.Validations;

namespace Shared.DataTransferObject.AdminDTOs.Dashboard
{
    public class ServiceRegisterDTO
    {
        [Required(ErrorMessage = "اسم الحرفة مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "يجب أن يكون اسم الحرفة بين 2 و 100 حرف")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "يمكن أن يحتوي اسم الحرفة على حروف ومسافات فقط")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "صورة الخدمة مطلوبة")]
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile ServiceImage { get; set; } = null!;
    }
}
