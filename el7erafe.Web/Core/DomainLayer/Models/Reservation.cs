
using DomainLayer.Models.IdentityModule.Enums;

namespace DomainLayer.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public ReservationStatus Status { get; set; }

        public DateTime? FinishedAt { get; set; }

        public int OfferId { get; set; }

        public Offer Offer { get; set; } = default!;
    }
}
