using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Sanara.UnitTests;

public class TestBase
{
    protected IServiceProvider _provider;

    [SetUp]
    public async Task Setup()
    {
        _provider = await Program.CreateProviderAsync(null, null, new Credentials()
        {
            VndbToken = Environment.GetEnvironmentVariable("VNDB_TOKEN"),
            Danbooru = new()
            {
                Username = Environment.GetEnvironmentVariable("DANBOORU_USERNAME"),
                ApiKey = Environment.GetEnvironmentVariable("DANBOORU_APIKEY")
            }
        }, false);
    }

    protected async Task AssertLinkAsync(string url)
    {
        Assert.That(url, Is.Not.Null);
        try
        {
            var req = await _provider.GetRequiredService<HttpClient>().SendAsync(new(HttpMethod.Head, url));
            Assert.That(req.IsSuccessStatusCode, Is.True, $"{url} failed to return success status code with {req.StatusCode}");
        }
        catch
        {
            Assert.Fail($"Call to {url} failed");
        }
    }
}
