using Discord;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game.GameMode
{
    public class TextMode : IGameMode
    {
        public async Task PostAsync(IMessageChannel chan, string text)
        {
            await chan.SendMessageAsync(text);
        }
    }
}
