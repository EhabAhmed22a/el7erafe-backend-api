
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ClientDTOs
{
    public class GetAvailableTechniciansRequest
    {
        [Required(ErrorMessage = "الخدمة مطلوبة")]
        public string ServiceName { get; set; } = default!;

        [Required(ErrorMessage = "اسم المدينة مطلوب")]
        public string CityName { get; set; } = default!;
        [Required]
        public DateOnly Day { get; set; }
        [Required]
        public TimeOnly FromTime { get; set; }
        [Required]
        public TimeOnly ToTime { get; set; }
        [Required]
        public bool Sorted { get; set; }
    }

}
