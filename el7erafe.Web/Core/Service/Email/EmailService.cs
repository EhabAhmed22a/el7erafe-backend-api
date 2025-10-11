using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using ServiceAbstraction;

namespace Service.Email
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger
            ,IConfiguration configuration)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string email, string otpCode)
        {
            var subject = "Your el7erafe Verification Code";
            var htmlMessage = CreateOtpEmailTemplate(otpCode);
            await SendEmailAsync(email, subject, htmlMessage);
        }

        private async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName ?? string.Empty, _settings.FromEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _settings.SmtpHost,
                    _settings.SmtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls
                    );

                await client.AuthenticateAsync(
                    _settings.SmtpUsername,
                    _settings.SmtpPassword
                    );

                await client.SendAsync(message);
                _logger.LogInformation("Email sent to {email}", email);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while using email service");
            }
        }

        private string CreateOtpEmailTemplate(string otpCode)
        {
            return $@"
            <table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='background-color: #111827; padding: 20px;'>
                <tr>
                    <td align='center'>
                        <table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='max-width: 600px; background-color: #1f2937; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.35);'>
                            <tr>
                                <td style='padding: 30px 40px; text-align: center;'>
                                    <h1 style='margin: 0; font-size: 24px; color: #10b981;'>رمز التحقق</h1>
                                    <p style='margin-top: 10px; font-size: 16px; color: #d1d5db;'>استخدم الرمز أدناه لإكمال العملية</p>
                                </td>
                            </tr>
                            <tr>
                                <td align='center' style='padding: 20px;'>
                                    <div style='background: #374151; color: #10b981; font-size: 32px; font-weight: bold; padding: 20px; border-radius: 8px; letter-spacing: 8px;'>
                                        {otpCode}
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td style='padding: 20px 40px; font-size: 12px; text-align: center; color: #6b7280;'>
                                    © 2025 الحرفة. جميع الحقوق محفوظة.
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>";
        }
    }
}
