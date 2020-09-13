using Discord;
using System.Threading.Tasks;

namespace SanaraV3.Game.PostMode
{
    public class UrlMode : IPostMode
    {
        public async Task PostAsync(IMessageChannel chan, string text, AGame _)
        {
            await chan.SendFileAsync(await StaticObjects.HttpClient.GetStreamAsync(text), "image.png");
        }
    }
}
