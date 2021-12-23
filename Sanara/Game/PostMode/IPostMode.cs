using Discord;

namespace Sanara.Game.PostMode
{
    public interface IPostMode
    {
        public Task PostAsync(IMessageChannel chan, string text, AGame sender);
    }
}
