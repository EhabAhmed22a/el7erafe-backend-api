using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
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

        public async Task<IEnumerable<Technician>> GetAllAsync()
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Technician>> GetAllByStatusAsync(TechnicianStatus status)
        {
            return await _context.Set<Technician>()
                .Include(t => t.User)
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        public async Task UpdateAsync(Technician technician)
        {
            _context.Set<Technician>().Update(technician);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var technician = await _context.Set<Technician>().FindAsync(id);
            if (technician != null)
            {
                _context.Set<Technician>().Remove(technician);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<Technician>().AnyAsync(t => t.Id == id);
        }

        public async Task<bool> ExistsByNationalIdAsync(string nationalId)
        {
            return await _context.Set<Technician>().AnyAsync(t => t.NationalId == nationalId);
        }
    }
}
