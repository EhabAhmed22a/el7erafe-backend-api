
using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IReservationRepository
    {
        Task AddAsync(Reservation reservation);
        Task<Reservation?> GetByOfferIdAsync(int offerId);
        Task<int> UpdateReservation(Reservation reservation);
        Task<List<Reservation>> GetCurrentReservationsAsync(int technicianId, DateTime date);
        Task<bool> IsReservationDone(int reservationId);
        Task<bool> IsReservationInPayment(int reservationId);
        Task<Reservation?> MarkReservationAsDone(int reservationId);
        Task<bool> IsReservationFound(int reservationId);
        Task<bool> IsReservationCancelled(int reservationId);
        Task<bool> CanCancelReservation(int reservationId);
        Task<Reservation?> CancelReservation(int reservationId,bool isClient);
        Task<Reservation?> GetByIdWithDetailsAsync(int reservationId);
        Task<List<Reservation>> GetCurrentReservationsAsync(int clientId);
        Task<List<Reservation>> GetPreviousReservationsAsync(int clientId);
        Task<List<Reservation>> GetInProgressReservationsAsync(int technicianId);
        Task<bool> HasEarlierUnfinishedReservations(int technicianId, Reservation currentReservation);
        Task<bool> HasActiveInProgressJob(int technicianId);
        Task<bool> IsReservationConfirmed(int reservationId);
        Task SaveChangesAsync();
    }
}
