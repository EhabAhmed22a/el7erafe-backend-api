
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.ClientDTOs
{
    public class GetAvailableTechniciansRequest
    {
        [Required(ErrorMessage = "الخدمة مطلوبة")]
        public string ServiceName { get; set; } = default!;

        [Required(ErrorMessage = "اسم المدينة مطلوب")]
        public string CityName { get; set; } = default!;

        public bool Sorted { get; set; } 
    }

}
