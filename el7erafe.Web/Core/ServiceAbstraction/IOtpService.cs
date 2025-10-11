
namespace ServiceAbstraction
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync<T>(string identifier, T data);
        Task<T?> VerifyOtpAsync<T>(string identifier, string otpCode) where T : class;
    }
}
