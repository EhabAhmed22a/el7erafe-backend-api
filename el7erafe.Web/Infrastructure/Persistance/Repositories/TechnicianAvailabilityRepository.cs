using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
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

        public async Task<ICollection<string>> GetAvailableTechsForRequestAsync(int serviceId, int govId, WeekDay date, TimeOnly? from, TimeOnly? to)
        {
            var query = dbContext.Set<TechnicianAvailability>()
                .AsQueryable();

            query = query.Where(a =>
                a.Technician.City.Governorate.Id == govId &&
                a.Technician.ServiceId == serviceId);


            query = query.Where(a => a.DayOfWeek == date || a.DayOfWeek == null);

            if (from.HasValue && to.HasValue)
            {
                var fromTime = from.Value;
                var toTime = to.Value;
                query = query.Where(a => fromTime <= a.ToTime && toTime >= a.FromTime);
            }
           
            var availableIds = await query
                .Where(a => !string.IsNullOrEmpty(a.Technician.User.Id))
                .Select(a => a.Technician.User.Id)
                .Distinct()
                .ToListAsync();

            return availableIds;
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
