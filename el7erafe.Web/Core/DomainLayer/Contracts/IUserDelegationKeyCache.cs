using Azure.Storage.Blobs.Models;

namespace DomainLayer.Contracts
{
    public interface IUserDelegationKeyCache
    {
        Task<UserDelegationKey> GetUserDelegationKeyAsync();
    }
}
