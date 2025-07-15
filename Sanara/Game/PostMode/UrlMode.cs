using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using Sanara.Exception;

namespace Sanara.Game.PostMode
{
    public class UrlMode : IPostMode
    {
        public async Task PostAsync(IServiceProvider provider, CommonMessageChannel chan, string text, AGame _)
        {
            try
            {
                var result = await provider.GetRequiredService<HttpClient>().SendAsync(new HttpRequestMessage(HttpMethod.Head, text));
                var length = int.Parse(result.Content.Headers.GetValues("content-length").ElementAt(0));
                if (length < 8000000)
                    await chan.SendFileAsync((await provider.GetRequiredService<HttpClient>().GetAsync(text)).Content.ReadAsStream(), "image" + Path.GetExtension(text));
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
