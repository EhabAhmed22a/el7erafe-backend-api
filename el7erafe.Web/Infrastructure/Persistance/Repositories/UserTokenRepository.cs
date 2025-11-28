using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class UserTokenRepository(ApplicationDbContext _dbContext) : IUserTokenRepository
    {
        public async Task CreateUserTokenAsync(UserToken userToken)
        {
            await _dbContext.UserTokens.AddAsync(userToken);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserToken?> GetUserTokenAsync(string userId)
        {
            return await _dbContext.UserTokens
                .Include(ut => ut.User)
                .FirstOrDefaultAsync(ut => ut.UserId == userId);
        }

        public async Task DeleteUserTokenAsync(string userId)
        {
            var userToken = await GetUserTokenAsync(userId);
            if (userToken != null)
            {
                _dbContext.UserTokens.Remove(userToken);
                await _dbContext.SaveChangesAsync();
            }
        }


        public async Task<bool> TokenExistsAsync(string token)
        {
            return await _dbContext.UserTokens.AnyAsync(ut => ut.Token == token);
        }

        public async Task<bool> UserHasTokenAsync(string userId)
        {
            return await _dbContext.UserTokens.AnyAsync(ut => ut.UserId == userId);
        }

        public async Task<UserToken?> GetByTokenAsync(string token)
        {
            return await _dbContext.UserTokens
            .FirstOrDefaultAsync(ut => ut.Token == token);
        }
    }
}
