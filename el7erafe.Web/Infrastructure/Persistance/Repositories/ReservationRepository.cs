using DomainLayer.Contracts;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;
using Service.Helpers;

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
                        .ThenInclude(sr => sr.Client)
                .FirstOrDefaultAsync(r => r.Id == reservationId);
        }

        public async Task<List<Reservation>> GetInProgressReservationsAsync(int technicianId)
        {
            return await dbcontext.Reservations
                .Where(r =>
                    r.Offer.TechnicianId == technicianId &&
                    r.Status == ReservationStatus.InProgress
                )
                .Include(r => r.Offer)
                    .ThenInclude(o => o.ServiceRequest)
                        .ThenInclude(sr => sr.Client)
                .Include(r => r.Offer.ServiceRequest.Service)
                .ToListAsync();
        }

        public async Task<bool> HasEarlierUnfinishedReservations(int technicianId, Reservation currentReservation)
        {
            return await dbcontext.Reservations
                .Where(r =>
                    r.Id != currentReservation.Id &&
                    r.Offer.TechnicianId == technicianId &&
                    r.Offer.ServiceRequest.ServiceDate == currentReservation.Offer.ServiceRequest.ServiceDate && // same day only
                    r.Offer.WorkFrom < currentReservation.Offer.WorkFrom && // earlier time
                    r.Status == ReservationStatus.Confirmed // NOT started yet
                )
                .AnyAsync();
        }

        public async Task<List<Reservation>> GetCurrentReservationsAsync(int clientId)
        {
            var activeStatuses = new[]
                {
                    ReservationStatus.InPayment,
                    ReservationStatus.Confirmed,
                    ReservationStatus.InProgress
                };
            return await dbcontext.Reservations
                    .AsNoTracking()
                    .Include(r => r.Offer)
                        .ThenInclude(o => o.ServiceRequest)
                            .ThenInclude(sr => sr.Service)
                    .Include(r => r.Offer)
                        .ThenInclude(o => o.Technician)
                    .Where(r =>
                        r.Offer.ServiceRequest.ClientId == clientId &&
                        activeStatuses.Contains(r.Status))
                    .OrderBy(r => r.Offer.ServiceRequest.ServiceDate)
                    .ToListAsync();
        }

        public async Task<bool> HasActiveInProgressJob(int technicianId)
        {
            return await dbcontext.Reservations
                .AnyAsync(r =>
                    r.Offer.TechnicianId == technicianId &&
                    r.Status == ReservationStatus.InProgress ||
                    r.Status == ReservationStatus.InPayment
                );
        }

        public async Task<int> UpdateReservation(Reservation reservation)
        {
            dbcontext.Reservations.Update(reservation);
            return await dbcontext.SaveChangesAsync();
        }

        public async Task<List<Reservation>> GetPreviousReservationsAsync(int clientId)
        {
            var historyStatuses = new[]
            {
                ReservationStatus.CancelledByClient,
                ReservationStatus.CancelledByTech,
                ReservationStatus.Done
            };

            return await dbcontext.Reservations
                .AsNoTracking()
                .Include(r => r.Offer)
                    .ThenInclude(o => o.ServiceRequest)
                        .ThenInclude(sr => sr.Service)
                .Include(r => r.Offer)
                    .ThenInclude(o => o.Technician)
                .Where(r => r.Offer.ServiceRequest.ClientId == clientId && historyStatuses.Contains(r.Status))
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await dbcontext.SaveChangesAsync();
        }

        public async Task<bool> IsReservationConfirmed(int clientId, int serviceId)
        {
            return await dbcontext.ServiceRequests
                .AnyAsync(s => s.ClientId == clientId && s.ServiceId == serviceId && s.Status == ServiceReqStatus.Reserved &&
                s.Offers.Any(o => o.Reservation != null && o.Reservation.Status == ReservationStatus.Confirmed));
        }

        public async Task<bool> IsReservationInPayment(int clientId)
        {
            return await dbcontext.ServiceRequests
                .AnyAsync(s => s.ClientId == clientId && s.Status == ServiceReqStatus.Reserved &&
                s.Offers.Any(o => o.Reservation != null && o.Reservation.Status == ReservationStatus.InPayment));
        }

        public async Task<bool> IsReservationInProgress(int clientId, int serviceId)
        {
            return await dbcontext.ServiceRequests
                .AnyAsync(s => s.ClientId == clientId && s.ServiceId == serviceId && s.Status == ServiceReqStatus.Reserved &&
                s.Offers.Any(o => o.Reservation != null && o.Reservation.Status == ReservationStatus.InProgress));
        }

        public async Task<bool> IsReservationAlreadyCancelled(int reservationId)
        {
            var statuses = new[]
            {
                ReservationStatus.CancelledByClient,
                ReservationStatus.CancelledByTech
            };
            return await dbcontext.Reservations.AnyAsync(r => r.Id == reservationId && statuses.Contains(r.Status));
        }

        public async Task<bool> CanCancelReservation(int reservationId)
        {
            var info = await dbcontext.Reservations
                .Where(r => r.Id == reservationId)
                .Select(r => new
                {
                    r.Status,
                    Date = r.Offer.ServiceRequest.ServiceDate,
                    StartTime = r.Offer.WorkFrom
                })
                .FirstOrDefaultAsync();

            if (info == null || info.Status != ReservationStatus.Confirmed)
                return false;

            if (info.StartTime == null)
                return false;

            DateTime appointmentTime = info.Date.ToDateTime(info.StartTime.Value);

            DateTime currentEgyptTime = HelperClass.ConvertUtcToEgyptTime(DateTime.UtcNow);

            TimeSpan timeRemaining = appointmentTime - currentEgyptTime;
            return timeRemaining.TotalHours >= 1;
        }

        public async Task<bool> IsReservationFound(int reservationId)
        {
            return await dbcontext.Reservations.AnyAsync(r => r.Id == reservationId);
        }

        public async Task CancelReservation(int reservationId, bool isClient)
        {
            var reservation = await dbcontext.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId);

            reservation!.Status = isClient ? ReservationStatus.CancelledByClient : ReservationStatus.CancelledByTech;
            await dbcontext.SaveChangesAsync();
        }
    }
}
