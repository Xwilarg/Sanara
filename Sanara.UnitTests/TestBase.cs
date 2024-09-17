using DiscordBotsList.Api.Internal;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Sanara.UnitTests;

public class TestBase
{
    protected IServiceProvider _provider;

    [SetUp]
    public async Task Setup()
    {
        _provider = await Program.CreateProviderAsync(null, null);
    }

    protected async Task<bool> AssertLinkAsync(string url)
    {
        try
        {
            var req = await _provider.GetRequiredService<HttpClient>().SendAsync(new(HttpMethod.Head, url));
            return req.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
