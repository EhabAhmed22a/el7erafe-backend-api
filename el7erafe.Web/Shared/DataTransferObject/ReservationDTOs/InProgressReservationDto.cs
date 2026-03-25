
namespace Shared.DataTransferObject.ReservationDTOs
{
    public class InProgressReservationDto
    {
        public int ReservationId { get; set; }

        public string? ClientName { get; set; }
        public string? ClientImage { get; set; }

        public DateOnly? Day { get; set; }

        public string? TechTimeInterval { get; set; }

        public string? Description { get; set; }

        public decimal Fees { get; set; }
    }
}
