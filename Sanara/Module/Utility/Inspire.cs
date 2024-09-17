namespace Sanara.Module.Utility;

public class Inspire
{
    public static async Task<string> GetInspireAsync(HttpClient client)
        => await client.GetStringAsync("https://inspirobot.me/api?generate=true");
}
