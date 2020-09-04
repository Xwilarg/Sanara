using Discord;
using Discord.Commands;
using SanaraV3.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadGameHelp()
        {
            _help.Add(new Help("Play", new[] { new Argument(ArgumentType.MANDATORY, "shiritori") }, "Play a game. Rules will be displayed when you start it.", false));
            _help.Add(new Help("Cancel", new Argument[0], "Cancel a game running in this channel.", false));
        }
    }
}

namespace SanaraV3.Modules.Game
{
    public sealed class GameModule : ModuleBase
    {
        [Command("Play")]
        public async Task PlayAsync(string gameName)
        {
            if (StaticObjects.Games.Any(x => x.IsMyGame(Context.Channel.Id)))
                await ReplyAsync("A game is already running in this channel.");
            else
            {
                var game = LoadGame(gameName.ToLowerInvariant(), Context.Channel);
                if (game == null)
                    await ReplyAsync("There is no game with this name.");
                else
                {
                    StaticObjects.Games.Add(game);
                    await game.StartAsync();
                }
            }
        }

        [Command("Cancel"), RequireRunningGame]
        public async Task CancelAsync()
        {
            var game = StaticObjects.Games.Find(x => x.IsMyGame(Context.Channel.Id));
            await game.CancelAsync();
        }

        public AGame LoadGame(string gameName, IMessageChannel textChan)
        {
            foreach (var preload in StaticObjects.Preloads)
            {
                if (preload.GetGameNames().Contains(gameName))
                    return preload.CreateGame(textChan, new GameSettings(false));
            }
            return null;
        }
    }
}
