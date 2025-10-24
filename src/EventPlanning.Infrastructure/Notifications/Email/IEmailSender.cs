namespace EventPlanning.Infrastructure.Notifications.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string html);
    }
}
