using Discord;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game
{
    public interface IGameMode
    {
        public Task PostAsync(IMessageChannel chan, string text);
    }
}
