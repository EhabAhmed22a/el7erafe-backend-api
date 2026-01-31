
using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class BlockedUserRepository(ApplicationDbContext dbContext) : IBlockedUserRepository
    {
        public async Task AddAsync(BlockedUser blockedUser)
        {
            await dbContext.Set<BlockedUser>().AddAsync(blockedUser);
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> IsPermBlockedAsync(string userId)
        {
            return await dbContext.Set<BlockedUser>().AnyAsync(c => c.EndDate == null && c.UserId == userId);
        }

        public async Task<BlockedUser?> GetByUserIdAsync(string userId)
        {
            return await dbContext.Set<BlockedUser>().FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<bool> IsBlockedAsync(string userId)
        {
            return await dbContext.Set<BlockedUser>().AnyAsync(u => u.UserId == userId && u.EndDate != null);
        }

        public async Task<bool> IsPermOrTempBlockedAsync(string userId)
        {
            return await dbContext.Set<BlockedUser>().AnyAsync(u => u.UserId == userId);
        }

        public async Task UpdateAsync(BlockedUser blockedUser)
        {
            dbContext.Set<BlockedUser>().Update(blockedUser);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveAsync(string userId)
        {
            var blockedAudit = await dbContext.Set<BlockedUser>().FirstOrDefaultAsync(u => u.UserId == userId);
            dbContext.Set<BlockedUser>().Remove(blockedAudit);
            await dbContext.SaveChangesAsync();
        }
    }
}
