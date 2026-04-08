namespace DomainLayer.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int Value { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}