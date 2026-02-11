using DomainLayer.Contracts;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class ServiceRequestRepository(ApplicationDbContext dbContext) : IServiceRequestRepository
    {
        public Task<ServiceRequest> GetServiceById(int id)
        {
            throw new NotImplementedException();
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

        public async Task<bool> IsServiceAlreadyReq(int? clientId, int? serviceId)
        {
            return await dbContext
                .Set<ServiceRequest>()
                .AnyAsync(sr => sr.ClientId == clientId && sr.ServiceId == serviceId);
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
