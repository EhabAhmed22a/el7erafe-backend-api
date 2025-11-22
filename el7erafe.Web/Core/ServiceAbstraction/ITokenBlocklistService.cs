
namespace ServiceAbstraction
{
    public interface ITokenBlocklistService
    {
        Task<bool> IsTokenRevokedAsync(string token);
        Task RevokeTokenAsync(string token, DateTime expiry);
    }
}
