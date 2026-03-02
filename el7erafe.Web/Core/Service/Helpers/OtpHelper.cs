
using DomainLayer.Models.IdentityModule;
using Microsoft.Extensions.Logging;
using Service.Email;
using ServiceAbstraction;

namespace Service.Helpers
{
    public class OtpHelper(IOtpService otpService, IEmailService emailService, ILogger<OtpHelper> logger)
    {
        public async Task SendOTP(ApplicationUser? user, string? customEmail = null)
        {

            var identifier = GetOtpIdentifier(user.Id);
            logger.LogInformation("[SERVICE] OTP generated for client verification: {UserId}", user.Id);
            var otpCode = await otpService.GenerateOtp(identifier);
            _ = Task.Run(async () =>
            {
                await emailService.SendOtpEmailAsync(customEmail ?? user.Email, otpCode);
            });
            logger.LogInformation("[SERVICE] OTP email sent to client: {Email}", user.Email);
        }

        public Task<bool> VerifyOtp(string id, string otp)
        {
            return otpService.VerifyOtp(id, otp);
        }

        public Task<bool> CanResendOtp(string id)
        {
            return otpService.CanResendOtp(id);
        }

        public string GetOtpIdentifier(string userId) => $"registration_{userId}";
    }
}
