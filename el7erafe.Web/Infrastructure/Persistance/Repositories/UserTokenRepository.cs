using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistance.Databases;

namespace Persistance.Repositories
{
    public class UserTokenRepository(ApplicationDbContext _dbContext,
                       ILogger<UserTokenRepository> _logger) : IUserTokenRepository
    {
        public async Task CreateUserTokenAsync(UserToken userToken)
        {
            var existingToken = await GetUserTokenAsync(userToken.UserId);

            if (existingToken != null)
            {
                existingToken.Token = userToken.Token;
                existingToken.Type = userToken.Type;
                existingToken.CreatedAt = DateTime.UtcNow;

                _dbContext.UserTokens.Update(existingToken);
                _logger.LogInformation("[REPO] Token updated for user {UserId}, type: {Type}",
                    userToken.UserId, userToken.Type);
            }
            else
            {
                await _dbContext.UserTokens.AddAsync(userToken);
                _logger.LogInformation("[REPO] Token created for user {UserId}, type: {Type}",
                    userToken.UserId, userToken.Type);
            }
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
                _logger.LogInformation("[REPO] Token deleted for user {UserId}", userId);
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
    }
}
