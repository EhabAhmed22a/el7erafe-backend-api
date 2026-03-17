using DomainLayer.Contracts;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class ReservationRepository(ApplicationDbContext dbcontext) : IReservationRepository
    {
        public async Task AddAsync(Reservation reservation)
        {
            await dbcontext.Reservations.AddAsync(reservation);
        }

        public async Task<Reservation?> GetByOfferIdAsync(int offerId)
        {
            return await dbcontext.Reservations
                .FirstOrDefaultAsync(r => r.OfferId == offerId);
        }

        public async Task<List<Reservation>> GetCurrentReservationsAsync(int technicianId, DateTime date)
        {
            var targetDate = DateOnly.FromDateTime(date);

            return await dbcontext.Reservations
                .Where(r =>
                    r.Status == ReservationStatus.Confirmed &&
                    r.Offer.TechnicianId == technicianId &&
                    r.Offer.ServiceRequest.ServiceDate == targetDate
                )
                .Include(r => r.Offer)
                    .ThenInclude(o => o.ServiceRequest)
                        .ThenInclude(sr => sr.Client)
                .Include(r => r.Offer.ServiceRequest.Service)
                .ToListAsync();
        }

        public async Task<Reservation?> GetByIdWithDetailsAsync(int reservationId)
        {
            return await dbcontext.Reservations
                .Include(r => r.Offer)
                    .ThenInclude(o => o.ServiceRequest)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task<bool> HasEarlierUnfinishedReservations(int technicianId, Reservation currentReservation)
        {
            return await dbcontext.Reservations
                .Where(r =>
                    r.Offer.TechnicianId == technicianId &&
                    r.Status != ReservationStatus.Done &&
                    r.Status != ReservationStatus.CancelledByClient &&
                    r.Status != ReservationStatus.CancelledByTech &&

                    // same day
                    r.Offer.ServiceRequest.ServiceDate == currentReservation.Offer.ServiceRequest.ServiceDate &&

                    // earlier time
                    r.Offer.WorkFrom < currentReservation.Offer.WorkFrom
                )
                .AnyAsync();
        }

        public async Task<int> UpdateReservation(Reservation reservation)
        {
            dbcontext.Reservations.Update(reservation);
            return await dbcontext.SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await dbcontext.SaveChangesAsync();
        }

    }
}
