using EventPlanning.Api;
using EventPlanning.Infrastructure.Notifications.Email;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IEmailSender, TestEmailSender>();
        });
    }
}

public class TestEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Html)> Sent = new();
    public Task SendAsync(string to, string subject, string html)
    { Sent.Add((to, subject, html)); return Task.CompletedTask; }
}