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
            _settings.SmtpPassword = configuration.GetSection("gmail")["SmtpPassword"]
                      ?? Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")!;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(string email, string confirmationLink)
        {
            var subject = "Verify your el7erafe account";

            var emailTemplate = @"
            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #111827; padding: 20px;"">
                <tr>
                    <td align=""center"">
                        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; background-color: #1f2937; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.35);"">
                            <tr>
                                <td style=""padding: 30px 40px; text-align: center;"">
                                    <h1 style=""margin: 0; font-size: 24px; color: #10b981;"">مرحباً بك في الحرفة!</h1>
                                    <p style=""margin-top: 10px; font-size: 16px; color: #d1d5db;"">أنت على بعد خطوة واحدة! يرجى تأكيد عنوان بريدك الإلكتروني لإكمال عملية التسجيل</p>
                                </td>
                            </tr>
                            <tr>
                                <td align=""center"" style=""padding: 20px;"">
                                    <a href=""{0}"" target=""_blank""
                                       style=""display: inline-block; background: linear-gradient(135deg, #059669 0%, #064e3b 100%);
                                       color: #f3f4f6; text-decoration: none; padding: 12px 24px; border-radius: 6px; font-weight: bold; font-size: 16px;"">
                                        تأكيد البريد الإلكتروني
                                    </a>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 0 40px 30px 40px; font-size: 14px; color: #9ca3af;"">
                                    <p>إذا لم يعمل الزر أعلاه، يرجى نسخ الرابط التالي ولصقه في المتصفح:</p>
                                    <p style=""word-break: break-all;""><a href=""{0}"" style=""color: #10b981; text-decoration: underline;"">{0}</a></p>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding: 20px 40px; font-size: 12px; text-align: center; color: #6b7280;"">
                                    © 2025 الحرفي. جميع الحقوق محفوظة.
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>";

            var message = string.Format(emailTemplate, confirmationLink);
            await SendEmailAsync(email, subject, message);
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
    }
}
