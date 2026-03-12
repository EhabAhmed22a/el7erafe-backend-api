using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class TechnicianAvailabilityRepository(ApplicationDbContext dbContext) : ITechnicianAvailabilityRepository
    {
        public async Task<TechnicianAvailability> CreateAsync(TechnicianAvailability availability)
        {
            await dbContext.Set<TechnicianAvailability>().AddAsync(availability);
            await dbContext.SaveChangesAsync();
            return availability;
        }

        public async Task<int> DeleteAsync(int id)
        {
            var availability = await dbContext.Set<TechnicianAvailability>().FindAsync(id);
            if (availability is not null)
            {
                dbContext.Set<TechnicianAvailability>().Remove(availability);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<int> DeleteByTechnicianIdAsync(int technicianId)
        {
            var availabilities = await dbContext.Set<TechnicianAvailability>()
                .Where(ta => ta.TechnicianId == technicianId)
                .ToListAsync();

            if (availabilities.Count > 0)
            {
                dbContext.Set<TechnicianAvailability>().RemoveRange(availabilities);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await dbContext.Set<TechnicianAvailability>().AnyAsync(ta => ta.Id == id);
        }

        public async Task<bool> ExistsForTechnicianAsync(int technicianId)
        {
            return await dbContext.Set<TechnicianAvailability>().AnyAsync(ta => ta.TechnicianId == technicianId);
        }

        public async Task<TechnicianAvailability?> GetByIdAsync(int id)
        {
            return await dbContext.Set<TechnicianAvailability>()
                .Include(ta => ta.Technician)
                .FirstOrDefaultAsync(ta => ta.Id == id);
        }

        public async Task<IEnumerable<TechnicianAvailability>?> GetByTechnicianIdAsync(int technicianId)
        {
            return await dbContext.Set<TechnicianAvailability>()
                .Where(ta => ta.TechnicianId == technicianId)
                .OrderBy(ta => ta.DayOfWeek)
                    .ThenBy(ta => ta.FromTime)
                .ToListAsync();
        }

        public async Task<int> UpdateAsync(TechnicianAvailability availability)
        {
            dbContext.Set<TechnicianAvailability>().Update(availability);
            return await dbContext.SaveChangesAsync();
        }
    }
}
