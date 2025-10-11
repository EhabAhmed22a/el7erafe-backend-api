
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;

namespace Service
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<OtpService> _logger;
        private readonly TimeSpan _otpExpiration = TimeSpan.FromMinutes(3);

        public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public Task<string> GenerateOtpAsync<T>(string identifier, T data)
        {
            var otpCode = GenerateOtpCode();
            var cacheKey = GetCacheKey(identifier, otpCode);

            _cache.Set(cacheKey, data, _otpExpiration);
            _logger.LogInformation("[OTP] Generated OTP for {Identifier}", identifier);
            return Task.FromResult(otpCode);
        }

        public Task<T?> VerifyOtpAsync<T>(string identifier, string otpCode) where T:class
        {
            var cacheKey = GetCacheKey(identifier, otpCode);

            if(!_cache.TryGetValue(cacheKey, out T? data))
            {
                _logger.LogWarning("[OTP] Invalid OTP for {Identifier}", identifier);
                return Task.FromResult<T?>(default);
            }

            _cache.Remove(cacheKey);
            _logger.LogInformation("[OTP] OTP verified for {Identifier}", identifier);

            return Task.FromResult<T?>(data);
        }

        private string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private static string GetCacheKey(string identifier, string otpCode)
            => $"OTP_{identifier}_{otpCode}";
    }
}
