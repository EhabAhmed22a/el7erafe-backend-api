
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Shared.Validations;

namespace Shared.DataTransferObject.ServiceRequestDTOs
{
    public class ServiceRequestRegDTO
    {
        [Required(ErrorMessage = "الوصف مطلوب")]
        [StringLength(1000, MinimumLength = 10,
            ErrorMessage = "الوصف يجب أن يكون بين ١٠ و ١٠٠٠ حرف")]
        public string Description { get; set; } = default!;

        [Required(ErrorMessage = "الخدمة مطلوبة")]
        [Range(1, int.MaxValue, ErrorMessage = "الرجاء اختيار خدمة صالحة")]
        public int? ServiceId { get; set; }

        [MaxLength(5, ErrorMessage = "الحد الأقصى للصور هو '٥'")]
        public List<IFormFile>? Images { get; set; }

        [Required(ErrorMessage = "المدينة مطلوبة")]
        public string? CityName { get; set; }

        [Required(ErrorMessage = "عنوان الشارع مطلوب")]
        [StringLength(200, MinimumLength = 5,
            ErrorMessage = "اسم الشارع يجب أن يكون بين ٥ و ٢٠٠ حرف")]
        public string Street { get; set; } = default!;

        [StringLength(500, ErrorMessage = "العلم الخاص لا يمكن أن يتجاوز ٥٠٠ حرف")]
        public string? SpecialSign { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        [FutureDate]
        public DateOnly? ServiceDate { get; set; }

        public bool AllDayAvailability { get; set; } = true;

        public TimeOnly? AvailableFrom { get; set; }
        public TimeOnly? AvailableTo { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!AllDayAvailability)
            {
                if (!AvailableFrom.HasValue || !AvailableTo.HasValue)
                {
                    yield return new ValidationResult(
                        "يجب تحديد نطاق وقت البداية والنهاية عندما لا تكون متاحاً طوال اليوم",
                        new[] { nameof(AvailableFrom), nameof(AvailableTo) });
                }
                else if (AvailableFrom.Value >= AvailableTo.Value)
                {
                    yield return new ValidationResult(
                        "وقت البداية يجب أن يكون قبل وقت النهاية",
                        new[] { nameof(AvailableFrom), nameof(AvailableTo) });
                }
                else if ((AvailableTo.Value - AvailableFrom.Value).TotalHours > 23)
                {
                    yield return new ValidationResult(
                        "إذا كنت متاحاً طوال اليوم، الرجاء اختيار 'متاح طوال اليوم'",
                        new[] { nameof(AvailableFrom), nameof(AvailableTo) });
                }
            }

            if (ServiceDate < DateOnly.FromDateTime(DateTime.Today))
            {
                yield return new ValidationResult(
                    "تاريخ الخدمة لا يمكن أن يكون في الماضي",
                    new[] { nameof(ServiceDate) });
            }
        }
    }
}
