
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
