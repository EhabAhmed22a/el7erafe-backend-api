
namespace ServiceAbstraction
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string toEmail, string confirmationLink);
    }
}
