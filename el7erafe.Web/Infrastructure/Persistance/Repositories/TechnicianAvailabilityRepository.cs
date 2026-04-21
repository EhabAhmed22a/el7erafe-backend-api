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

        public Task CreateDefaultAvailabilityForTechnicianAsync(int technicianId)
        {
            var defaultAvailability = new TechnicianAvailability
            {
                TechnicianId = technicianId,
                DayOfWeek = null,
                FromTime = TimeOnly.Parse("00:00:00"),
                ToTime = TimeOnly.Parse("23:59:00")
            };
            return CreateAsync(defaultAvailability);
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

        public async Task<ICollection<string>> GetAvailableTechsForRequestAsync(
            int serviceId,
            int govId,
            WeekDay date,
            TimeOnly? requestedFrom,
            TimeOnly? requestedTo,
            TimeOnly? minimumStartTime) 
        {
            var query = dbContext.Set<TechnicianAvailability>().AsQueryable();

            query = query.Where(a =>
                a.Technician.City.Governorate.Id == govId &&
                a.Technician.ServiceId == serviceId);

            query = query.Where(a => a.DayOfWeek == date || a.DayOfWeek == null);

            // 1. FILTER OUT DEAD SHIFTS (The 5 PM vs 1 PM problem)
            if (minimumStartTime.HasValue)
            {
                // The tech's shift MUST end AFTER the request was created.
                // This instantly kills the 1 PM tech if the request was made at 5 PM!
                query = query.Where(a => a.ToTime > minimumStartTime.Value);
            }

            // 2. CHECK SPECIFIC TIME REQUESTS (Encapsulation)
            if (requestedFrom.HasValue && requestedTo.HasValue)
            {
                var fromTime = requestedFrom.Value;
                var toTime = requestedTo.Value;

                query = query.Where(a => a.FromTime <= fromTime && a.ToTime >= toTime);
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
