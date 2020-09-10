using Discord;
using Discord.Commands;
using SanaraV3.Attributes;
using SanaraV3.Games;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadGameHelp()
        {
            _help.Add(new Help("Play", new[] { new Argument(ArgumentType.MANDATORY, "shiritori/arknights"), new Argument(ArgumentType.OPTIONAL, "audio") }, "Play a game. Rules will be displayed when you start it.", false));
            _help.Add(new Help("Cancel", new Argument[0], "Cancel a game running in this channel.", false));
            _help.Add(new Help("Replay", new Argument[0], "Replay the audio for the current game.", false));
        }
    }
}

namespace SanaraV3.Modules.Game
{
    public sealed class GameModule : ModuleBase
    {
        [Command("Play", RunMode = RunMode.Async)]
        public async Task PlayAsync(string gameName, string mode = null)
        {
            if (StaticObjects.Games.Any(x => x.IsMyGame(Context.Channel.Id)))
                await ReplyAsync("A game is already running in this channel.");
            else
            {
                var game = LoadGame(gameName.ToLowerInvariant(), Context.Channel, Context.User, mode);
                if (game == null)
                    await ReplyAsync("There is no game with this name.");
                else
                {
                    StaticObjects.Games.Add(game);
                    await game.StartAsync();
                }
            }
        }

        [Command("Cancel", RunMode = RunMode.Async), RequireRunningGame]
        public async Task CancelAsync()
        {
            var game = StaticObjects.Games.Find(x => x.IsMyGame(Context.Channel.Id));
            await game.CancelAsync();
        }

        [Command("Replay"), RequireRunningGame]
        public async Task ReplayAsync()
        {
            var game = StaticObjects.Games.Find(x => x.IsMyGame(Context.Channel.Id));
            await game.ReplayAsync();
        }

        public AGame LoadGame(string gameName, IMessageChannel textChan, IUser user, string argument)
        {
            foreach (var preload in StaticObjects.Preloads)
            {
                if (preload.GetGameNames().Contains(gameName) && preload.GetNameArg() == argument)
                    return preload.CreateGame(textChan, user, new GameSettings(false));
            }
            return null;
        }
    }
}
