using Discord;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game.GameMode
{
    public class UrlMode : IGameMode
    {
        public async Task PostAsync(IMessageChannel chan, string text)
        {
            await chan.SendFileAsync(await StaticObjects.HttpClient.GetStreamAsync(text), "image.png");
        }
    }
}
