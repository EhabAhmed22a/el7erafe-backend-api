
using DomainLayer.Models;

namespace DomainLayer.Contracts
{
    public interface IReservationRepository
    {
        Task AddAsync(Reservation reservation);
        Task<Reservation?> GetByOfferIdAsync(int offerId);
        Task SaveChangesAsync();
    }
}
