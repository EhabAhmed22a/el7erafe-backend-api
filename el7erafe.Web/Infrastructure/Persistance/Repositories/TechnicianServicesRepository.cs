
using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class TechnicianServicesRepository(ApplicationDbContext dbContext) : ITechnicianServicesRepository
    {
        public async Task<IEnumerable<TechnicianService>?> GetAllTechnicianServicesAsync()
        {
            return await dbContext.Set<TechnicianService>()
                                    .ToListAsync();
        }

        public async Task<IEnumerable<TechnicianService>?> GetPagedTechnicianServicesAsync(int pageNumber, int pageSize)
        {
            return await dbContext.Set<TechnicianService>()
                                    .OrderBy(ts => ts.NameAr)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
        }

        public async Task<TechnicianService> CreateServiceAsync(TechnicianService technicianService)
        {
            await dbContext.Set<TechnicianService>().AddAsync(technicianService);
            await dbContext.SaveChangesAsync();
            return technicianService;
        }

        public async Task<bool> ServiceExistsAsync(string serivceName)
        {
            return await dbContext.Set<TechnicianService>().AnyAsync(ts => ts.NameAr == serivceName);
        }
    }
}
