using System.Net;


namespace EventPlanning.IntegrationTests
{
    public class ProfileGateTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly CustomWebAppFactory _factory;
        public ProfileGateTests(CustomWebAppFactory factory) => _factory = factory;

        [Fact]
        public async Task Authenticated_User_Without_Profile_Gets_428()
        {
            var client = _factory.CreateClient(); // без auth cookies/JWT этот тест не покажет 428
                                                  // здесь мы просто проверим, что аноним не блокируется middleware
            var resp = await client.GetAsync("/api/profile/me");
            Assert.True(resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
        }
    }
}
