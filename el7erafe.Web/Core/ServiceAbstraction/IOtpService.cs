
namespace ServiceAbstraction
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string identifier);
        Task<bool> VerifyOtpAsync(string identifier, string otpCode);
    }
}
