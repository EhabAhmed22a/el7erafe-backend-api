
namespace Shared.DataTransferObject.ReservationDTOs
{
    public class PreviousReservationsDTO
    {
        public string? techName {  get; set; }
        public string? techImage {  get; set; }
        public string? serviceType {  get; set; }
        public decimal? fees {  get; set; }
        public string? techTimeInterval { get; set; }
        public DateOnly day {  get; set; }
        public bool IsCancelled { get; set; }   
    }
}
