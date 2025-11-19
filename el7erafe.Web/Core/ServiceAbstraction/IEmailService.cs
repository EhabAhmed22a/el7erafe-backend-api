
namespace ServiceAbstraction
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string email, string otpCode);
    }
}
