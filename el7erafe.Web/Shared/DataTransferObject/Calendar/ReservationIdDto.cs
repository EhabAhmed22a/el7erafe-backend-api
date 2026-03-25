
using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObject.Calendar
{
    public class ReservationIdDto
    {
        [Required]
        public int ReservationId { get; set; }
    }
}
