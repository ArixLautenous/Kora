using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.Json;

namespace KX_Project.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpUser;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _refreshToken;

        public EmailSender()
        {
            _smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
            _clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "";
            _clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "";
            _refreshToken = Environment.GetEnvironmentVariable("GOOGLE_REFRESH_TOKEN") ?? "";
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "refresh_token", _refreshToken },
                { "grant_type", "refresh_token" }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseString);
                if (jsonDoc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
                {
                    return accessTokenElement.GetString();
                }
            }

            Console.WriteLine($"Error refreshing token: {await response.Content.ReadAsStringAsync()}");
            return null;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_smtpUser) || string.IsNullOrEmpty(_refreshToken))
            {
                Console.WriteLine("Gmail API (OAuth2) is not fully configured. Email was not sent.");
                return;
            }

            var accessToken = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Failed to get Access Token from Google.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("KoraStore", _smtpUser));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                
                // Sử dụng cơ chế OAuth 2.0 thay vì mật khẩu thông thường
                var oauth2 = new SaslMechanismOAuth2(_smtpUser, accessToken);
                await client.AuthenticateAsync(oauth2);
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email via Gmail API: {ex.Message}");
            }
        }
    }
}
