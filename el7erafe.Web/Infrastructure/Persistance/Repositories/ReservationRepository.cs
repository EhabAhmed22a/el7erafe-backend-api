using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class ReservationRepository(ApplicationDbContext dbcontext) : IReservationRepository
    {
        public async Task AddAsync(Reservation reservation)
        {
            await dbcontext.Reservations.AddAsync(reservation);
            await dbcontext.SaveChangesAsync();
        }

        public async Task<Reservation?> GetByOfferIdAsync(int offerId)
        {
            return await dbcontext.Reservations
                .FirstOrDefaultAsync(r => r.OfferId == offerId);
        }
    }
}
