using System.Net;
using System.Net.Mail;

namespace OtobusBiletSistemi.Web.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Şifre Sıfırlama - Otobüs Bilet Sistemi";
            var htmlContent = GeneratePasswordResetEmailHtml(resetLink);
            
            await SendEmailAsync(toEmail, subject, htmlContent);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "Otobüs Bilet Sistemi";

                // Email ayarları kontrolü
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || 
                    string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
                {
                    throw new InvalidOperationException("Email ayarları tamamlanmamış. Lütfen appsettings.json dosyasındaki EmailSettings konfigürasyonunu kontrol edin.");
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email başarıyla gönderildi: {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Email gönderme hatası: {toEmail}");
                throw;
            }
        }

        private string GeneratePasswordResetEmailHtml(string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #2c3e50; color: white !important; text-align: center; padding: 30px 20px; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .btn {{ display: inline-block; background: #007bff; color: white !important; padding: 15px 35px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; border: 2px solid #007bff; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; color: #856404; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚌 Otobüs Bilet Sistemi</h1>
            <h2>Şifre Sıfırlama</h2>
        </div>
        <div class='content'>
            <p>Merhaba,</p>
            <p>Hesabınız için şifre sıfırlama talebinde bulundunuz. Aşağıdaki bağlantıya tıklayarak yeni şifrenizi belirleyebilirsiniz:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetLink}' class='btn'>Şifremi Sıfırla</a>
            </div>
            
            <div class='warning'>
                <strong>⚠️ Önemli Uyarılar:</strong>
                <ul>
                    <li>Bu bağlantı sadece 1 saat geçerlidir</li>
                    <li>Bağlantı tek kullanımlıktır</li>
                    <li>Bu talebi siz yapmadıysanız, bu emaili görmezden gelebilirsiniz</li>
                </ul>
            </div>
            
            <p>Eğer yukarıdaki bağlantıya tıklayamıyorsanız, aşağıdaki adresi tarayıcınıza kopyalayıp yapıştırabilirsiniz:</p>
            <p style='word-break: break-all; background: #e9ecef; padding: 10px; border-radius: 4px; font-size: 12px;'>{resetLink}</p>
        </div>
        <div class='footer'>
            <p>Bu email otomatik olarak gönderilmiştir. Lütfen yanıtlamayın.</p>
            <p>&copy; 2024 Otobüs Bilet Sistemi. Tüm hakları saklıdır.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
} 