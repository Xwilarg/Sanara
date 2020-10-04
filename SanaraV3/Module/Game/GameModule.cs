using Discord;
using Discord.Commands;
using SanaraV3.Attribute;
using SanaraV3.Exception;
using SanaraV3.Game;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadGameHelp()
        {
            _submoduleHelp.Add("Game", "Play various games directly on Discord");
            _help.Add(("Game", new Help("Game", "Play", new[] { new Argument(ArgumentType.MANDATORY, "shiritori/arknights/kancolle/girlsfrontline/fatego/pokemon/anime"), new Argument(ArgumentType.OPTIONAL, "audio") }, "Play a game. Rules will be displayed when you start it.", new string[0], Restriction.None, "Play arknights audio")));
            _help.Add(("Game", new Help("Game", "Cancel", new Argument[0], "Cancel a game running in this channel.", new string[0], Restriction.None, null)));
            _help.Add(("Game", new Help("Game", "Replay", new Argument[0], "Replay the audio for the current game.", new string[0], Restriction.None, null)));
            _help.Add(("Game", new Help("Game", "Delete cache", new Argument[0], "Delete the cache of a game.", new string[0], Restriction.OwnerOnly, null)));
        }
    }
}

namespace SanaraV3.Module.Game
{
    public sealed class GameModule : ModuleBase
    {
        [Command("Delete cache", RunMode = RunMode.Async), RequireOwner]
        public async Task DeleteCache(string gameName)
        {
            if (await StaticObjects.Db.DeleteCacheAsync(gameName))
                await ReplyAsync("The cache for this game was deleted, please restart me so I can download it back.");
            else
                await ReplyAsync("There is no cache loaded for this name.");
        }

        [Command("Score")]
        public Task ScoreAsync()
        {
            throw new NotYetAvailable();
        }

        [Command("Play", RunMode = RunMode.Async)]
        public async Task PlayAsync(string gameName, string mode = null)
        {
            if (mode == "multi" || mode == "multiplayer" || gameName == "booru")
                throw new NotYetAvailable();
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
