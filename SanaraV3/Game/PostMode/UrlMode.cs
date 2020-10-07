using Discord;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV3.Game.PostMode
{
    public class UrlMode : IPostMode
    {
        public async Task PostAsync(IMessageChannel chan, string text, AGame _)
        {
            var result = await StaticObjects.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, text));
            var length = int.Parse(result.Content.Headers.GetValues("content-length").ElementAt(0));
            if (length < 8000000)
                await chan.SendFileAsync(await StaticObjects.HttpClient.GetStreamAsync(text), "image" + Path.GetExtension(text));
            else // Too big to be sent on Discord
                await chan.SendMessageAsync(text);
        }
    }
}
