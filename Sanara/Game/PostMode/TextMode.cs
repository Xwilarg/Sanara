using Discord;

namespace Sanara.Game.PostMode
{
    public class TextMode : IPostMode
    {
        public async Task PostAsync(IServiceProvider _, IMessageChannel chan, string text, AGame _2)
        {
            await chan.SendMessageAsync(text);
        }
    }
}
