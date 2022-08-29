using Discord;
using Sanara.Exception;

namespace Sanara.Game.PostMode
{
    public class UrlMode : IPostMode
    {
        public async Task PostAsync(IMessageChannel chan, string text, AGame _)
        {
            try
            {
                var result = await StaticObjects.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, text));
                var length = int.Parse(result.Content.Headers.GetValues("content-length").ElementAt(0));
                if (length < 8000000)
                    await chan.SendFileAsync((await StaticObjects.HttpClient.GetAsync(text)).Content.ReadAsStream(), "image" + Path.GetExtension(text));
                else // Too big to be sent on Discord
                    await chan.SendMessageAsync(text);
            }
            catch (Discord.Net.HttpException dne)
            {
                if (dne.DiscordCode == DiscordErrorCode.MissingPermissions)
                {
                    throw new GameLost("Missing permissions to send files");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
