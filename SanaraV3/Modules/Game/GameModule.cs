using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game
{
    public sealed class GameModule : ModuleBase, IModule
    {
        public string GetModuleName()
            => "Game";

        [Command("Play")]
        public async Task Play(string gameName)
        {
            if (StaticObjects.Games.Any(x => x.IsMyGame(Context.Channel.Id)))
                await ReplyAsync("A game is already running in this channel.");
            else
                StaticObjects.Games.Add(LoadGame(gameName, Context.Channel));
        }

        public AGame LoadGame(string gameName, IMessageChannel textChan)
        {
            return null;
        }
    }
}
