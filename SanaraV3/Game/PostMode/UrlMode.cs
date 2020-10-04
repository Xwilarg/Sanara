using Discord;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV3.Game.PostMode
{
    public class UrlMode : IPostMode
    {
        public async Task PostAsync(IMessageChannel chan, string text, AGame _)
        {
            await chan.SendFileAsync(await StaticObjects.HttpClient.GetStreamAsync(text), "image" + Path.GetExtension(text));
        }
    }
}
