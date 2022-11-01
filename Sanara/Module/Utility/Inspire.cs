namespace Sanara.Module.Utility
{
    public class Inspire
    {
        public static async Task<string> GetInspireAsync()
            => await StaticObjects.HttpClient.GetStringAsync("https://inspirobot.me/api?generate=true");
    }
}
