
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;

namespace Service.Email
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
        public Task<string> GenerateOtpAsync(string identifier)
        {
            var otpCode = GenerateOtpCode();
            var cacheKey = GetCacheKey(identifier, otpCode);

            _cache.Set(cacheKey, true, _otpExpiration);
            _logger.LogInformation("[OTP] Generated OTP for {Identifier}", identifier);
            return Task.FromResult(otpCode);
        }

        public Task<bool> VerifyOtpAsync(string identifier, string otpCode)
        {
            var cacheKey = GetCacheKey(identifier, otpCode);

            if(!_cache.TryGetValue(cacheKey, out _))
            {
                _logger.LogWarning("[OTP] Invalid OTP for {Identifier}", identifier);
                return Task.FromResult(false);
            }

            _cache.Remove(cacheKey);
            _logger.LogInformation("[OTP] OTP verified for {Identifier}", identifier);

            return Task.FromResult(true);
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
