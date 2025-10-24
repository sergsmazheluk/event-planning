using EventPlanning.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace EventPlanning.IntegrationTests
{
    public class EventsApiTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public EventsApiTests(CustomWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task Create_And_Get_Events_Works()
        {
            var client = _factory.CreateClient();

            // 1) тип события
            var defReq = new
            {
                name = "Интеграционный тип",
                fields = new[] { new { key = "location", label = "Локация", type = "string", required = false } }
            };
            var defResp = await client.PostAsJsonAsync("/api/admin/events/definitions", defReq);
            defResp.EnsureSuccessStatusCode();

            var defJson = await defResp.Content.ReadFromJsonAsync<JsonElement>();
            var defId = defJson.TryGetProperty("id", out var p) ? p.GetGuid() : defJson.GetProperty("Id").GetGuid();

            // 2) событие
            var evReq = new
            {
                eventDefinitionId = defId,
                startsAtUtc = System.DateTime.UtcNow.AddDays(1),
                capacity = 50,
                customDataJson = "{\"location\":\"Minsk\"}"
            };
            var createResp = await client.PostAsJsonAsync("/api/admin/events", evReq);
            createResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            // 3) список
            var listResp = await client.GetAsync("/api/events");
            listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
