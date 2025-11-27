using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface IUserTokenRepository
    {
        Task<UserToken> GetUserTokenAsync(string userId); 
        Task<UserToken> GetByTokenAsync(string token);
        Task CreateUserTokenAsync(UserToken userToken);
        Task DeleteUserTokenAsync(string userId);
        Task<bool> TokenExistsAsync(string token);
        Task<bool> UserHasTokenAsync(string userId); 
    }
}
