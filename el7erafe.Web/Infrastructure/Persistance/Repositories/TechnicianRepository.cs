using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using DomainLayer.Models.IdentityModule.Enums;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class TechnicianRepository(ApplicationDbContext context) : ITechnicianRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Technician> CreateAsync(Technician technician)
        {
            await _context.Set<Technician>().AddAsync(technician);
            await _context.SaveChangesAsync();
            return technician;
        }

        public async Task<Technician?> GetByIdAsync(int id)
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Technician?> GetByUserIdAsync(string userId)
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<Technician?> GetFullTechnicianByUserIdAsync(string userId)
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .Include(t => t.Rejection)
                .Include(t => t.City)
                    .ThenInclude(c => c.Governorate)
                .Include(t => t.Service)
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task<IEnumerable<Technician>?> GetAllAsync()
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .Include(c => c.Service)
                .Include(c => c.City)
                    .ThenInclude(city => city.Governorate)
                .Where(t => t.Status != TechnicianStatus.Pending)
                .ToListAsync();
        }

        public async Task<IEnumerable<Technician>?> GetAllByStatusAsync(TechnicianStatus status)
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .Include(t => t.Service)
                .Include(t => t.City)
                    .ThenInclude(city => city.Governorate)
                .OrderBy(t => t.User.CreatedAt)
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        public async Task<int> UpdateAsync(Technician technician)
        {
            _context.Set<Technician>().Update(technician);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(string id)
        {
            var technician = await _context.Set<ApplicationUser>().FindAsync(id);
            if (technician is not null)
            {
                _context.Set<ApplicationUser>().Remove(technician);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<Technician>().AnyAsync(t => t.Id == id);
        }

        public async Task<bool> ExistsAsync(string phoneNumber)
        {
            return await _context.Set<ApplicationUser>().AnyAsync(t => t.UserName == phoneNumber);
        }

        public async Task<IEnumerable<TechnicianService>?> GetAllServicesAsync()
        {
            return await _context.Set<TechnicianService>()
                .ToListAsync();
        }

        public async Task<Governorate?> GetGovernorateByNameAsync(string nameAr)
        {
            return await _context.Set<Governorate>()
                .FirstOrDefaultAsync(g => g.NameAr == nameAr);
        }

        public async Task<City?> GetCityByNameAsync(string nameAr, int governorateId)
        {
            return await _context.Set<City>()
               .FirstOrDefaultAsync(c => c.NameAr == nameAr && c.GovernorateId == governorateId);
        }

        public async Task<TechnicianService?> GetServiceByNameAsync(string nameAr)
        {
            return await _context.Set<TechnicianService>()
                .FirstOrDefaultAsync(s => s.NameAr == nameAr);
        }

        public async Task<IEnumerable<Technician>?> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await context.Set<Technician>()
                .Include(c => c.User)
                .Include(c => c.Service)
                .Include(c => c.City)
                    .ThenInclude(city => city.Governorate)
                .Where(t => t.Status != TechnicianStatus.Pending)
                .OrderBy(c => c.User.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Technician>?> GetPagedByStatusAsync(TechnicianStatus status, int pageNumber, int pageSize)
        {
            return await context.Set<Technician>()
                .Include(c => c.User)
                .Include(c => c.Service)
                .Include(c => c.City)
                    .ThenInclude(city => city.Governorate)
                .Where(c => c.Status == status)
                .OrderBy(c => c.User.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Set<ApplicationUser>().AnyAsync(t => t.Email == email);
        }

        public async Task<IEnumerable<Technician>?> GetAvailableApprovedTechniciansWithSortingAsync(int governorateId, int preferredCityId, bool sorted)
        {
            var query = context.Set<Technician>()
                               .Include(t => t.User)
                               .Include(t => t.Rejection)
                               .Include(t => t.City)
                                   .ThenInclude(c => c.Governorate)
                               .Include(t => t.Service)
                               .Where(t => t.City.GovernorateId == governorateId && t.Status == TechnicianStatus.Accepted);
            if (sorted)
            {
                return await query.OrderByDescending(t => t.Rating)
                                    .ThenBy(t => t.City.NameEn)
                                  .ToListAsync();

            }
            return await query.OrderBy(t => t.CityId == preferredCityId ? 0 : 1)
                                .ThenByDescending(t => t.Rating)
                                    .ThenBy(t => t.City.NameEn)
                              .ToListAsync();
        }
    }
}
