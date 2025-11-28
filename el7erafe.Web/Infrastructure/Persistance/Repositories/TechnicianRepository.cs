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

        public async Task<IEnumerable<Technician>?> GetAllAsync()
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Technician>?> GetAllByStatusAsync(TechnicianStatus status)
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        public async Task<int> UpdateAsync(Technician technician)
        {
            _context.Set<Technician>().Update(technician);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            var technician = await _context.Set<Technician>().FindAsync(id);
            if (technician is not null)
            {
                _context.Set<Technician>().Remove(technician);
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


    }
}
