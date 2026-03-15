
using DomainLayer.Contracts;
using DomainLayer.Models;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class IgnoredServiceRequestsRepository(ApplicationDbContext dbContext) : IIgnoredServiceRequestsRepository
    {
        public async Task<IgnoredServiceRequest> CreateAsync(IgnoredServiceRequest request)
        {
            await dbContext.Set<IgnoredServiceRequest>().AddAsync(request);
            await dbContext.SaveChangesAsync();
            return request;
        }

        public async Task<bool> DeleteAllByTechnicianId(int techId)
        {
            int deletedRows = await dbContext.Set<IgnoredServiceRequest>()
                                           .Where(i => i.TechnicianId == techId)
                                           .ExecuteDeleteAsync();

            return deletedRows > 0;
        }

        public async Task<bool> IsAlreadyIgnoredAsync(int techId, int serviceId)
        {
            return await dbContext.Set<IgnoredServiceRequest>()
                .AnyAsync(i => i.TechnicianId == techId && i.ServiceRequestId == serviceId);
        }
    }
}
