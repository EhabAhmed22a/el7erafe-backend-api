
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServiceAbstraction;

namespace Service.Email
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<OtpService> _logger;
        private readonly TimeSpan _otpExpiration;

        public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
        {
            _cache = cache;
            _logger = logger;
            _otpExpiration = TimeSpan.FromMinutes(1);
        }
        public Task<string> GenerateOtpAsync(string identifier)
        {
            var otpCode = GenerateOtpCode();
            var cacheKey = GetCacheKey(identifier, otpCode);
            var trackingKey = GetTrackingKey(identifier);

            _cache.Set(cacheKey, true, _otpExpiration);
            _cache.Set(trackingKey, true, _otpExpiration);
            _logger.LogInformation("[OTP] Generated OTP for {Identifier}", identifier);
            return Task.FromResult(otpCode);
        }

        public Task<bool> VerifyOtpAsync(string identifier, string otpCode)
        {
            var cacheKey = GetCacheKey(identifier, otpCode);
            var trackingKey = GetTrackingKey(identifier);

            if(!_cache.TryGetValue(cacheKey, out _))
            {
                _logger.LogWarning("[OTP] Invalid OTP for {Identifier}", identifier);
                return Task.FromResult(false);
            }

            _cache.Remove(cacheKey);
            _cache.Remove(trackingKey);
            _logger.LogInformation("[OTP] OTP verified for {Identifier}", identifier);

            return Task.FromResult(true);
        }

        public Task<bool> CanResendOtpAsync(string identifier)
        {
            var trackingKey = GetTrackingKey(identifier);

            _logger.LogInformation("[OTP] Can resend OTP for {Identifier}", identifier);
            return Task.FromResult(!_cache.TryGetValue(trackingKey, out _));
        }

        private string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private static string GetCacheKey(string identifier, string otpCode)
            => $"OTP_{identifier}_{otpCode}";

        private static string GetTrackingKey(string identifier)
            => $"OTP_Sent_{identifier}";
    }
}
