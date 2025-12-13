using Microsoft.AspNetCore.Http;
using Shared.Validations;
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.TechnicianIdentityDTOs
{
    public class TechRegisterDTO
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "يجب أن يكون الاسم بين 2 و 100 حرف")]
        [RegularExpression(@"^[a-zA-Z\u0600-\u06FF\s]+$", ErrorMessage = "يمكن أن يحتوي الاسم على حروف ومسافات فقط")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "يرجى إدخال رقم هاتف مصري صحيح (11 رقماً يبدأ بـ 010, 011, 012, أو 015)")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "يجب أن يكون رقم الهاتف 11 رقماً بالضبط")]
        public string PhoneNumber { get; set; } = default!;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = default!;

        [Required(ErrorMessage = "صورة بطاقة الرقم القومي (الوجه الأمامي) مطلوبة")]
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile NationalIdFront { get; set; } = default!;

        [Required(ErrorMessage = "صورة بطاقة الرقم القومي (الوجه الخلفي) مطلوبة")]
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile NationalIdBack { get; set; } = default!;

        [Required(ErrorMessage = "صورة ورقة الحالة الجنائية مطلوبة")]
        [ValidateFile(1 * 1024 * 1024, new[] { ".png", ".jpg", ".jpeg" })]
        public IFormFile CriminalRecord { get; set; } = default!;

        [Required(ErrorMessage = "نوع الخدمة مطلوب")]
        public string ServiceType { get; set; } = default!;

        [Required(ErrorMessage = "المحافظة مطلوبة")]
        public string Governorate { get; set; } = default!;

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string City { get; set; } = default!;
    }
}