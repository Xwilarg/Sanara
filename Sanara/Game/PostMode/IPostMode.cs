using Discord;

namespace Sanara.Game.PostMode
{
    public interface IPostMode
    {
        public Task PostAsync(IServiceProvider provider, IMessageChannel chan, string text, AGame sender);
    }
}
