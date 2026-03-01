using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DomainLayer.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Persistance
{
    public class UserDelegationKeyCache : IUserDelegationKeyCache
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserDelegationKeyCache> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromDays(6); // Cache for 6 days (keys valid for 7 days max)

        public UserDelegationKeyCache(
            BlobServiceClient blobServiceClient,
            IMemoryCache cache,
            ILogger<UserDelegationKeyCache> logger)
        {
            _blobServiceClient = blobServiceClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<UserDelegationKey> GetUserDelegationKeyAsync()
        {
            const string cacheKey = "UserDelegationKey";

            // Try to get from cache
            if (_cache.TryGetValue(cacheKey, out UserDelegationKey? cachedKey) && cachedKey != null)
            {
                _logger.LogDebug("Returning cached user delegation key. Expires on: {ExpiresOn}",
                    cachedKey.SignedExpiresOn);
                return cachedKey;
            }

            // Generate new key
            _logger.LogInformation("Generating new user delegation key");

            var now = DateTimeOffset.UtcNow;
            var expiry = now.AddDays(7); // Key valid for 7 days max [citation:2]

            // The GetUserDelegationKeyAsync method returns Response<UserDelegationKey> [citation:2]
            Response<UserDelegationKey> response = await _blobServiceClient.GetUserDelegationKeyAsync(
                startsOn: now,      // Can be null for immediate start [citation:2]
                expiresOn: expiry,
                cancellationToken: default
            );

            UserDelegationKey userDelegationKey = response.Value;

            _logger.LogInformation("User delegation key generated. " +
                "Starts: {StartsOn}, Expires: {ExpiresOn}, ObjectId: {ObjectId}, TenantId: {TenantId}",
                userDelegationKey.SignedStartsOn,
                userDelegationKey.SignedExpiresOn,
                userDelegationKey.SignedObjectId,
                userDelegationKey.SignedTenantId);

            // Cache for 6 days (leaving 1 day buffer before actual expiry)
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_cacheDuration)
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, userDelegationKey, cacheOptions);

            return userDelegationKey;
        }
    }
}