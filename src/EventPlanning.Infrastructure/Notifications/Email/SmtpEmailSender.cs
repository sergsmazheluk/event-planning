using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EventPlanning.Infrastructure.Notifications.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;

        public async Task SendAsync(string to, string subject, string html)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_cfg["Email:From"]));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_cfg["Email:SmtpHost"], int.Parse(_cfg["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_cfg["Email:User"], _cfg["Email:Password"]);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}
