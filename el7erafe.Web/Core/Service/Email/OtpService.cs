
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
        private readonly TimeSpan _resendTime;

        public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
        {
            _cache = cache;
            _logger = logger;
            _otpExpiration = TimeSpan.FromMinutes(3);
            _resendTime = TimeSpan.FromMinutes(1);
        }

        public Task<string> GenerateOtp(string identifier)
        {
            var otpCode = GenerateOtpCode();
            var otpKey = GetOtpKey(identifier);
            var trackingKey = GetTrackingKey(identifier);

            _cache.Set(otpKey, otpCode, _otpExpiration);
            _cache.Set(trackingKey, true, _resendTime);
            _logger.LogInformation("[OTP] Generated OTP for {Identifier}", identifier);
            return Task.FromResult(otpCode);
        }

        public Task<bool> VerifyOtp(string identifier, string otpCode)
        {
            var otpKey = GetOtpKey(identifier);
            var trackingKey = GetTrackingKey(identifier);

            if(_cache.TryGetValue(otpKey, out string? storedOtp) && storedOtp == otpCode)
            {
                _cache.Remove(otpKey);
                _cache.Remove(trackingKey);
                _logger.LogInformation("[OTP] OTP verified for {Identifier}", identifier);
                return Task.FromResult(true);
            }

            _logger.LogWarning("[OTP] Invalid OTP for {Identifier}", identifier);
            return Task.FromResult(false);
        }

        public Task<bool> CanResendOtp(string identifier)
        {
            var trackingKey = GetTrackingKey(identifier);
            if(_cache.TryGetValue(trackingKey, out _))
            {
                _logger.LogInformation("[OTP] Cannot resend OTP for {Identifier} - recently sent", identifier);
                return Task.FromResult(false);
            }

            var otpKey = GetOtpKey(identifier);
            if(_cache.TryGetValue(otpKey, out _))
            {
                _cache.Remove(otpKey);
                _cache.Remove(trackingKey);
                _logger.LogInformation("[OTP] Deleted old OTP for {Identifier}", identifier);
            }
            return Task.FromResult(true);
        }

        private string GenerateOtpCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string GetOtpKey(string identifier) => $"OTP_{identifier}";

        private string GetTrackingKey(string identifier)
            => $"OTP_Sent_{identifier}";
    }
}
