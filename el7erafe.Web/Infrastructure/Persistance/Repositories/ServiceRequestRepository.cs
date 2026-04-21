using DomainLayer.Contracts;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class ServiceRequestRepository(ApplicationDbContext dbContext) : IServiceRequestRepository
    {
        public async Task<ServiceRequest?> GetServiceById(int id)
        {
            return await dbContext.Set<ServiceRequest>()
                .Include(sr => sr.City)
                .Include(sr => sr.Service)
                .Include(sr => sr.Client)
                .Include(sr => sr.Technician)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public Task<TimeOnly?> GetServiceTime(int clientId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsTimeConflicted(int clientId, TimeOnly? AvailableFrom, TimeOnly? AvailableTo, DateOnly Date)
        {
            return await dbContext
                    .Set<ServiceRequest>()
                    .AnyAsync(sr => sr.ClientId == clientId && sr.AvailableFrom.OverlapsWith(sr.AvailableTo, AvailableFrom, AvailableTo) && sr.ServiceDate == Date);
        }

        public async Task<bool> IsServicePending(int? clientId, int? serviceId)
        {
            return await dbContext
                .Set<ServiceRequest>()
                .AnyAsync(sr => sr.ClientId == clientId && sr.ServiceId == serviceId && sr.Status == ServiceReqStatus.Pending);
        }

        public async Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest)
        {
            await dbContext.Set<ServiceRequest>().AddAsync(serviceRequest);
            await dbContext.SaveChangesAsync();
            return serviceRequest;
        }

        public async Task<bool> UpdateAsync(ServiceRequest serviceRequest)
        {
            var existingService = await dbContext.Set<ServiceRequest>()
            .FirstOrDefaultAsync(c => c.Id == serviceRequest.Id);

            if (existingService is null)
                return false;

            dbContext.Entry(existingService).CurrentValues.SetValues(existingService);

            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAsync(int id)
        {
            var serviceRequest = await dbContext.Set<ServiceRequest>().FindAsync(id);
            if (serviceRequest is not null)
            {
                dbContext.Set<ServiceRequest>().Remove(serviceRequest);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<IEnumerable<int>> GetServiceRequestIdsByClientAsync(int clientId)
        {
            return await dbContext
                .Set<ServiceRequest>()
                .Where(sr => sr.ClientId == clientId)
                .Select(sr => sr.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetServiceRequestIdsByTechnicianAsync(int techId)
        {
            return await dbContext
                .Set<ServiceRequest>()
                .Where(sr => sr.TechnicianId == techId)
                .Select(sr => sr.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByClientAsync(int clientId)
        {
            return await dbContext
                .Set<ServiceRequest>()
                .Where(sr => sr.ClientId == clientId)
                .Include(s => s.Offers)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceRequest>> GetAvailableServiceRequestsByTechnicianAsync(int techId, int serviceId, int govId)
        {
            var techSchedule = await dbContext.Set<TechnicianAvailability>()
                .Where(ta => ta.TechnicianId == techId)
                .ToListAsync();

            var potentialRequests = await dbContext.Set<ServiceRequest>()
                .Include(sr => sr.Technician)
                .Include(s => s.Service)
                .Include(s => s.Client)
                .Include(s => s.City)
                    .ThenInclude(c => c.Governorate)
                .Where(sr =>
                    sr.Status == ServiceReqStatus.Pending &&
                    (
                        (sr.TechnicianId == techId)
                        ||
                        (sr.TechnicianId == null && sr.ServiceId == serviceId && sr.City.GovernorateId == govId)
                    )
                    && !dbContext.Set<Offer>().Any(o =>
                            o.TechnicianId == techId &&
                            o.ServiceRequestId == sr.Id)
                    && !dbContext.Set<IgnoredServiceRequest>().Any(ignored =>
                            ignored.TechnicianId == techId &&
                            ignored.ServiceRequestId == sr.Id)
                )
                .ToListAsync();

            var now = DateTime.UtcNow; // Standardize on UTC or pass from service if needed
            // However, for time comparisons in TimeOnly, we should align with the system's timezone (Egypt)
            // Since this is a repository, we'll use a simple comparison or ideally pass the 'now' time.
            
            var validRequests = potentialRequests.Where(sr =>
            {
                var requestDayMapped = (int)((int)sr.ServiceDate.DayOfWeek + 1) % 7 + 1;
                var isToday = sr.ServiceDate == DateOnly.FromDateTime(now.AddHours(2)); // Rough Egypt adjustment or just use Today

                return techSchedule.Any(ta =>
                {
                    // 1. Day Match
                    if (!(ta.DayOfWeek == null || (int)ta.DayOfWeek == requestDayMapped))
                        return false;

                    // 2. Time Match
                    if (sr.AvailableFrom == null && sr.AvailableTo == null)
                    {
                        // ALL DAY Case: If it's today, tech must have shift time remaining
                        if (isToday)
                        {
                            var currentTime = TimeOnly.FromDateTime(now.AddHours(2));
                            return ta.ToTime > currentTime;
                        }
                        return true;
                    }
                    else
                    {
                        // Specific Time Case
                        return sr.AvailableFrom <= ta.ToTime && sr.AvailableTo >= ta.FromTime;
                    }
                });
            }).ToList();

            return validRequests;
        }

        public async Task<IEnumerable<ServiceRequest>> GetPendingServiceRequestsByClientAsync(int clientId)
        {
            return await dbContext
                .Set<ServiceRequest>()
                .Where(sr => sr.ClientId == clientId && sr.Status == ServiceReqStatus.Pending)
                .Include(s => s.Offers.Where(o => o.Status == OfferStatus.Pending))
                .Include(s => s.Service)
                .Include(s => s.Technician)
                .ToListAsync();
        }

        public async Task<bool> DeleteServiceRequestsByTechnicianIdAsync(int techId)
        {
            int deletedRows = await dbContext.Set<ServiceRequest>()
                                     .Where(sr => sr.TechnicianId == techId)
                                     .ExecuteDeleteAsync();

            return deletedRows > 0;
        }

        public async Task UpdateStatusAsync(int requestId, ServiceReqStatus status)
        {
            await dbContext.ServiceRequests
                   .Where(sr => sr.Id == requestId)
                   .ExecuteUpdateAsync(setters => setters
                       .SetProperty(sr => sr.Status, status));
        }

        public async Task<bool> IsReservationConfirmed(int clientId, int serviceId)
        {
            return await dbContext.ServiceRequests
                .AnyAsync(s => s.ClientId == clientId && s.ServiceId == serviceId && s.Status == ServiceReqStatus.Reserved &&
                s.Offers.Any(o => o.Reservation != null && o.Reservation.Status == ReservationStatus.Confirmed));
        }

        public async Task<bool> IsReservationInPayment(int clientId)
        {
            return await dbContext.ServiceRequests
                .AnyAsync(s => s.ClientId == clientId && s.Status == ServiceReqStatus.Reserved &&
                s.Offers.Any(o => o.Reservation != null && o.Reservation.Status == ReservationStatus.InPayment));
        }

        public async Task<bool> IsReservationInProgress(int clientId, int serviceId)
        {
            return await dbContext.ServiceRequests
                .AnyAsync(s => s.ClientId == clientId && s.ServiceId == serviceId && s.Status == ServiceReqStatus.Reserved &&
                s.Offers.Any(o => o.Reservation != null && o.Reservation.Status == ReservationStatus.InProgress));
        }
    }

    public static class TimeOnlyExtensions
    {
        public static bool OverlapsWith(this TimeOnly? start1, TimeOnly? end1, TimeOnly? start2, TimeOnly? end2)
        {
            return start1.HasValue && end1.HasValue && start2.HasValue && end2.HasValue &&
                   start1.Value < end2 && start2 < end1.Value;
        }
    }
}

