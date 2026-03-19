
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
        public bool AllDayAvailable { get; set; }

        public TimeOnly? FromTime { get; set; }

        public TimeOnly? ToTime { get; set; }
        [Required]
        public bool Sorted { get; set; }
    }

}
