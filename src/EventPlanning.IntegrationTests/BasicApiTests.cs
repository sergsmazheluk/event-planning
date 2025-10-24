using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using EventPlanning.Infrastructure.Notifications.Email;
using EventPlanning.Api;

namespace EventPlanning.IntegrationTests
{
    public class BasicApiTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BasicApiTests(CustomWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task App_Starts_And_Swagger_Is_Available_Or_NotFound()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/swagger/index.html");
            resp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Register_Sends_Email_Confirmation_Link()
        {
            var client = _factory.CreateClient();

            var dto = new { Email = $"u{System.Guid.NewGuid():N}@example.com", Password = "P@ssw0rd!" };
            var resp = await client.PostAsJsonAsync("/api/auth/register", dto);
            resp.EnsureSuccessStatusCode();

            var sender = _factory.Services.GetRequiredService<IEmailSender>() as TestEmailSender;
            sender.Should().NotBeNull();
            sender!.Sent.Should().NotBeEmpty();
            sender.Sent[^1].To.Should().Be(dto.Email);
            sender.Sent[^1].Subject.Should().Contain("Confirm");
            sender.Sent[^1].Html.Should().ContainAny("confirm", "Confirm");
            sender.Sent[^1].Html.Should().Contain("http");
        }
    }
}
