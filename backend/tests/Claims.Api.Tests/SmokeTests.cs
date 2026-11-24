using System.Net;
using Claims.Api.Tests.Infrastructure;

namespace Claims.Api.Tests;

[Collection(ApiCollection.Name)]
public class SmokeTests(ClaimsApiFactory factory)
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")] // includes the PostgreSQL check
    [InlineData("/api/system/info")]
    public async Task Endpoint_returns_ok(string url)
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
