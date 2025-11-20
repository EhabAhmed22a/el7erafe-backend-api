
namespace ServiceAbstraction
{
    public interface IOtpService
    {
        Task<string> GenerateOtp(string identifier);
        Task<bool> VerifyOtp(string identifier, string otpCode);
        Task<bool> CanResendOtp(string identifier);
    }
}
