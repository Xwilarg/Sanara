using Discord;
using Sanara.Compatibility;

namespace Sanara.Game.PostMode
{
    public class TextMode : IPostMode
    {
        public async Task PostAsync(IServiceProvider _, CommonMessageChannel chan, string text, AGame _2)
        {
            await chan.SendMessageAsync(text);
        }
    }
}
