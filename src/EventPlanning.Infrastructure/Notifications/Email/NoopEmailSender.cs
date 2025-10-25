using Microsoft.Extensions.Logging;

namespace EventPlanning.Infrastructure.Notifications.Email
{
    public sealed class NoopEmailSender : IEmailSender
    {
        private readonly ILogger<NoopEmailSender> _logger;
        public NoopEmailSender(ILogger<NoopEmailSender> logger) => _logger = logger;

        public Task SendAsync(string to, string subject, string html)
        {
            _logger.LogInformation("DEV-EMAIL ▶ To: {To} | Subj: {Subject}\n{Html}", to, subject, html);
            return Task.CompletedTask;
        }
    }
}
