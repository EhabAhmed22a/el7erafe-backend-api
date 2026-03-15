
using DomainLayer.Contracts;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class OffersRepository(ApplicationDbContext dbContext) : IOffersRepository
    {
        public async Task<Offer> AddOfferAsync(Offer offer)
        {
            await dbContext.Offers.AddAsync(offer);
            await dbContext.SaveChangesAsync();
            return offer;
        }

        public async Task<bool> HasTechnicianAlreadyOffered(int technicianId, int requestId)
        {
            return await dbContext.Offers
                .AnyAsync(o =>
                    o.TechnicianId == technicianId &&
                    o.ServiceRequestId == requestId);
        }

        public async Task<IEnumerable<Offer>> GetValidOffersForClientAsync(int serReqId, int clientId, bool isQuick)
        {
            return await dbContext.Set<Offer>()
                .Include(o => o.Technician)
                .Include(o => o.ServiceRequest)
                    .ThenInclude(sr => sr.Service)
                .Where(o =>
                    o.ServiceRequestId == serReqId &&
                    o.ServiceRequest.ClientId == clientId &&
                    o.ServiceRequest.Status == ServiceReqStatus.Pending &&
                    (isQuick
                        ? o.ServiceRequest.TechnicianId == null
                        : o.ServiceRequest.TechnicianId != null))
                .ToListAsync();
        }

        public async Task<bool> HasTimeConflict(int technicianId, TimeOnly fromTime, TimeOnly toTime, DateOnly serviceDate, int? numberOfDays)
        {
            var endDate = numberOfDays.HasValue
                ? serviceDate.AddDays(numberOfDays.Value)
                : serviceDate;

            return await dbContext.ServiceRequests
                .Where(r =>
                    r.TechnicianId == technicianId &&
                    r.Status == ServiceReqStatus.Reserved &&
                    r.ServiceDate >= serviceDate &&
                    r.ServiceDate <= endDate)
                .AnyAsync(r =>
                    fromTime < r.AvailableTo &&
                    toTime > r.AvailableFrom);
        }
    }
}
