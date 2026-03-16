
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

        public async Task<List<Offer>> GetPendingOffersForTechAsync(int technicianId)
        {
            return await dbContext.Set<Offer>()
                .Include(o => o.ServiceRequest)
                    .ThenInclude(sr => sr.Client)
                .Include(o => o.ServiceRequest)
                    .ThenInclude(sr => sr.Service)
                .Include(o => o.ServiceRequest)
                    .ThenInclude(sr => sr.City)
                .Where(o =>
                    o.TechnicianId == technicianId &&
                    o.ServiceRequest.Status == ServiceReqStatus.Pending)
                .ToListAsync();
        }

        public async Task<Offer?> GetByIdAsync(int offerId)
        {
            return await dbContext.Offers
                .Include(o => o.Technician)
                .Include(o => o.ServiceRequest)
                    .ThenInclude(sr => sr.Client)
                .Include(o => o.ServiceRequest)
                    .ThenInclude(sr => sr.Service)
                .FirstOrDefaultAsync(o => o.Id == offerId);
        }
    }
}
