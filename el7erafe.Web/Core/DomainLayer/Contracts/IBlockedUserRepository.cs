
using DomainLayer.Models.IdentityModule;

namespace DomainLayer.Contracts
{
    public interface IBlockedUserRepository
    {
        Task<bool> IsBlockedAsync(string userId);
        Task<bool> IsPermBlockedAsync(string userId);
        Task<BlockedUser?> GetByUserIdAsync(string userId);
        Task UpdateAsync(BlockedUser blockedUser);
        Task AddAsync(BlockedUser blockedUser);
        Task RemoveAsync(string userId);
    }
}
