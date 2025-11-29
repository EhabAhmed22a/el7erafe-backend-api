
using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class AdminRepository(ApplicationDbContext dbContext) : IAdminRepository
    {
        public async Task<Admin?> GetByUserId(string userId)
        {
            return await dbContext.Set<Admin>().Include(a => a.User).FirstOrDefaultAsync();
        }
    }
}
