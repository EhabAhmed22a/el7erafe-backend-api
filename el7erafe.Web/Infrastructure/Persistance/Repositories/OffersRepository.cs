
using DomainLayer.Contracts;
using DomainLayer.Models;
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

        public Task<bool> HasTimeConflict(int technicianId, TimeSpan fromTime, TimeSpan toTime, DateTime serviceDate, int? numberOfDays)
        {
            throw new NotImplementedException();
        }
    }
}
