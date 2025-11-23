using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;

namespace Service
{
    public class TokenBlocklistService(IMemoryCache _cache,
                       ILogger<TokenBlocklistService> _logger) : ITokenBlocklistService
    {

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            try
            {
                var isRevoked = _cache.TryGetValue(GetCacheKey(token), out _);
                _logger.LogDebug("[TOKEN] Checking if token is revoked: {IsRevoked}", isRevoked);
                return await Task.FromResult(isRevoked);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TOKEN] Error checking token revocation status");
                return false;
            }
        }

        public async Task RevokeTokenAsync(string token, DateTime expiry)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = expiry
                };

                _cache.Set(GetCacheKey(token), true, cacheOptions);
                _logger.LogInformation("[TOKEN] Token revoked until: {Expiry}", expiry);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TOKEN] Error revoking token");
                throw;
            }
        }

        private static string GetCacheKey(string token)
        {
            return $"revoked_token:{token}";
        }
    }
}
