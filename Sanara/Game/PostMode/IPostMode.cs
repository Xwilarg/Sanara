using Discord;
using Sanara.Compatibility;

namespace Sanara.Game.PostMode
{
    public interface IPostMode
    {
        public Task PostAsync(IServiceProvider provider, CommonMessageChannel chan, string text, AGame sender);
    }
}
