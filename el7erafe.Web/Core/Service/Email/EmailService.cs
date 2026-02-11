using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _settings = settings.Value;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _logger.LogInformation("[SERVICE] EmailService initialized with SMTP host: {SmtpHost}", _settings.SmtpHost);
        }

        public async Task SendOtpEmailAsync(string email, string otpCode)
        {
            _logger.LogInformation("[SERVICE] Starting OTP email sending process for: {Email}", email);

            try
            {
                var subject = "Your el7erafe Verification Code";
                _logger.LogDebug("[SERVICE] Creating OTP email template for OTP: {OtpCode}", otpCode);

                var htmlMessage = CreateOtpEmailTemplate(otpCode);
                _logger.LogDebug("[SERVICE] OTP email template created successfully");

                await SendEmailAsync(email, subject, htmlMessage);
                _logger.LogInformation("[SERVICE] OTP email sent successfully to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SERVICE] Failed to send OTP email to {Email}", email);
            }
        }

        private async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogDebug("[SERVICE] Starting email sending process to: {Email}, Subject: {Subject}", email, subject);

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName ?? string.Empty, _settings.FromEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                _logger.LogDebug("[SERVICE] MimeMessage created. From: {FromEmail}, To: {ToEmail}",
                    _settings.FromEmail, email);

                using var client = new SmtpClient();

                _logger.LogInformation("[SERVICE] Connecting to SMTP server: {SmtpHost}:{SmtpPort}",
                    _settings.SmtpHost, _settings.SmtpPort);

                await client.ConnectAsync(
                    _settings.SmtpHost,
                    _settings.SmtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls
                );

                _logger.LogDebug("[SERVICE] SMTP connection established successfully");

                _logger.LogInformation("[SERVICE] Authenticating with SMTP server using username: {Username}",
                    _settings.SmtpUsername);

                await client.AuthenticateAsync(
                    _settings.SmtpUsername,
                    _settings.SmtpPassword
                );

                _logger.LogDebug("[SERVICE] SMTP authentication successful");

                _logger.LogInformation("[SERVICE] Sending email to {Email}", email);
                await client.SendAsync(message);

                _logger.LogInformation("[SERVICE] Email sent successfully to {Email}", email);

                await client.DisconnectAsync(true);
                _logger.LogDebug("[SERVICE] Disconnected from SMTP server");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SERVICE] Error while sending email to {Email}. SMTP Host: {SmtpHost}, Port: {SmtpPort}, Username: {Username}",
                    email, _settings.SmtpHost, _settings.SmtpPort, _settings.SmtpUsername);
            }
        }

        private string CreateOtpEmailTemplate(string otpCode)
        {
            _logger.LogDebug("[SERVICE] Creating OTP email template");

            return $@"
    <!DOCTYPE html>
    <html lang=""ar"" dir=""rtl"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>رمز التحقق لخدمة الحرفي</title>
    </head>
    <body style=""margin:0;padding:0;direction:rtl;font-family:'Segoe UI',Arial,sans-serif;background-color:#f3f4f6;"">
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border-collapse:collapse;"">
            <tr>
                <td align=""center"" style=""padding:16px;"">
                    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""500"" style=""width:100%;max-width:500px;border-collapse:collapse;background-color:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #dbeafe;"">
                        <tr>
                            <td style=""padding:24px 24px 0 24px;text-align:center;vertical-align:middle;"">
                                <img src=""https://el7erafe.blob.core.windows.net/services-documents/logo-blue-wt.png"" alt=""الحرفي"" width=""34"" height=""33"" style=""display:inline-block;vertical-align:middle;border:0;"" />
                                <h1 style=""display:inline-block;vertical-align:middle;margin:0 0 0 8px;font-size:24px;font-weight:600;color:#0f172a;"">الحرفي</h1>
                            </td>
                        </tr>
                        <tr><td style=""font-size:0;line-height:0;padding:8px 0;"">&nbsp;</td></tr>
                        <tr>
                            <td style=""padding:0 24px 24px 24px;text-align:center;border:1px solid #d1d5db;border-radius:8px;"">
                                <div style=""margin:0 auto 16px auto;max-width:100%;"">
                                    <img src=""https://el7erafe.blob.core.windows.net/services-documents/image_59.png"" alt=""تصميم توضيحي"" width=""250"" height=""150"" style=""display:block;margin:0 auto;max-width:100%;height:auto;border:0;"" />
                                </div>
                                <h2 style=""margin:8px 0;font-size:24px;color:#0f172a;font-weight:600;"">مرحباً !</h2>
                                <p style=""margin:0 0 16px 0;line-height:1.4;font-size:16px;color:#334155;"">شكرا لاستخدامكم تطبيق الحرفي، رمز التحقق الخاص بك هو:</p>
                                <div style=""display:block;padding:16px 36px;border-radius:12px;margin:12px 0;background-color:#2563eb;color:#ffffff;font-weight:700;font-size:24px;text-align:center;"">{otpCode}</div>
                                <p style=""margin:16px 0 0 0;font-size:14px;line-height:1.4;color:#303741;"">يرجى إدخال هذا الرمز في التطبيق لإتمام عملية تسجيل الدخول أو التحقق من حسابك.</p>
                            </td>
                        </tr>
                        <tr><td style=""font-size:0;line-height:0;padding:12px 0;"">&nbsp;</td></tr>
                        <tr>
                            <td style=""padding:0 24px 24px 24px;text-align:center;"">
                                <p style=""margin:0;font-size:12px;line-height:1.4;color:rgb(69, 69, 69);font-weight:600;"">الحرفي | جميع الحقوق محفوظة @ 2025</p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";
        }
    }
}