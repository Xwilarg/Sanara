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
            {
                var game = LoadGame(gameName.ToLower(), Context.Channel);
                if (game == null)
                    await ReplyAsync("There is no game with this name.");
                else
                {
                    StaticObjects.Games.Add(game);
                    await game.Start();
                }
            }
        }

        public AGame LoadGame(string gameName, IMessageChannel textChan)
        {
            foreach (var preload in StaticObjects.Preloads)
            {
                if (preload.GetGameNames().Contains(gameName))
                    return preload.CreateGame(textChan);
            }
            return null;
        }
    }
}
