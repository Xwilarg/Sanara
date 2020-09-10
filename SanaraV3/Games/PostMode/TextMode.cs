using Discord;
using System.Threading.Tasks;

namespace SanaraV3.Games.PostMode
{
    public class TextMode : IPostMode
    {
        public async Task PostAsync(IMessageChannel chan, string text, AGame _)
        {
            await chan.SendMessageAsync(text);
        }
    }
}
