using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class RejectionRepository(ApplicationDbContext _dbContext,
        ILogger<RejectionRepository> _logger) : IRejectionRepository
    {
        public async Task<int> CreateAsync(Rejection rejection)
        {
            await _dbContext.Rejections.AddAsync(rejection);
            _logger.LogInformation("Rejection added successfully for technician: {TechnicianId}", rejection.TechnicianId);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            var rejection = await _dbContext.Rejections.FindAsync(id);
            if (rejection is not null)
            {
                _dbContext.Rejections.Remove(rejection);
                _logger.LogInformation("Rejection deleted successfully: {RejectionId}", id);
                return await _dbContext.SaveChangesAsync();
            }
            return 0;

        }

        public async Task<bool> ExistsForTechnicianAsync(int technicianId)
        {
            return await _dbContext.Rejections.AnyAsync(r => r.TechnicianId == technicianId);
        }

        public async Task<Rejection?> GetByIdAsync(int id)
        {
            return await _dbContext.Rejections
                .Include(r => r.Technician)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<int> UpdateAsync(Rejection rejection)
        {
            _dbContext.Rejections.Update(rejection);
            _logger.LogInformation("Rejection updated successfully: {RejectionId}", rejection.Id);
            return await _dbContext.SaveChangesAsync();

        }
    }
}
