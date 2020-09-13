using Discord;
using System.Threading.Tasks;

namespace SanaraV3.Game.PostMode
{
    public interface IPostMode
    {
        public Task PostAsync(IMessageChannel chan, string text, AGame sender);
    }
}
