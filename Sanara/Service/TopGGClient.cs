using DiscordBotsList.Api;

namespace Sanara.Service;

public class TopGGClient
{
    private AuthDiscordBotListApi _client;
    private DateTime _lastSend;

    public TopGGClient(ulong id, string token)
    {
        _client = new AuthDiscordBotListApi(id, token);
    }

    public bool ShouldSend => _lastSend.AddMinutes(10).CompareTo(DateTime.Now) < 0;

    public async Task SendAsync(int guildCount)
    {
        _lastSend = DateTime.Now;
        await _client.UpdateStats(guildCount);
    }
}
