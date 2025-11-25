
namespace DomainLayer.Models.IdentityModule
{
    public interface IUserTokenRepository
    {
        Task<UserToken> GetTokenAsync(string token);
        Task<UserToken> GetUserTokenAsync(string userId);
        Task<ApplicationUser> GetUserByTokenAsync(string token);
        Task CreateTokenAsync(UserToken userToken);
        Task UpdateTokenAsync(UserToken userToken);
        Task DeleteTokenAsync(string token);
        Task DeleteUserTokenAsync(string userId);
        Task<bool> TokenExistsAsync(string token);
        Task<bool> UserHasTokenAsync(string userId);
    }
}
