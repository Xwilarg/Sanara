using Discord;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game.PostMode
{
    public interface IPostMode
    {
        public Task PostAsync(IMessageChannel chan, string text, AGame sender);
    }
}
