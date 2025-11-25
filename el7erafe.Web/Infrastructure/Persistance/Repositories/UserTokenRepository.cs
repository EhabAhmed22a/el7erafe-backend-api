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
        public Task CreateTokenAsync(UserToken userToken)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserTokenAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<UserToken> GetTokenAsync(string token)
        {
            return await _dbContext.UserTokens
                .Include(ut => ut.User)
                .FirstOrDefaultAsync(ut => ut.Token == token);
        }

        public Task<ApplicationUser> GetUserByTokenAsync(string token)
        {
            throw new NotImplementedException();
        }

        public Task<UserToken> GetUserTokenAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TokenExistsAsync(string token)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTokenAsync(UserToken userToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserHasTokenAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
