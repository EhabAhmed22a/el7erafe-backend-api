
using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IReservationRepository
    {
        Task AddAsync(Reservation reservation);
        Task<Reservation?> GetByOfferIdAsync(int offerId);
        Task<List<Reservation>> GetCurrentReservationsAsync(int technicianId, DateTime date);
        Task<Reservation?> GetByIdWithDetailsAsync(int reservationId);
        Task<bool> HasEarlierUnfinishedReservations(int technicianId, Reservation currentReservation);
        Task SaveChangesAsync();
    }
}
