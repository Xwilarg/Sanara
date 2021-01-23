using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using SanaraV3.Attribute;
using SanaraV3.Exception;
using SanaraV3.Game;
using SanaraV3.Game.Custom;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Impl;
using SanaraV3.Game.Preload.Result;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadGameHelp()
        {
            _submoduleHelp.Add("Game", "Play various games directly on Discord");
            _help.Add(("Game", new Help("Game", "Play", new[] { new Argument(ArgumentType.MANDATORY, "shiritori / arknights / kancolle / girlsfrontline / fatego / pokemon / anime / booruquizz / boorufill"), new Argument(ArgumentType.OPTIONAL, "audio / multiplayer") }, "Play a game. Rules will be displayed when you start it. Audio parameter is only available for Arknights and KanColle quizzes.", new string[0], Restriction.None, "Play arknights audio")));
            _help.Add(("Game", new Help("Game", "Score", new Argument[0], "Display your score and ranking for all games.", new string[0], Restriction.None, null)));
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
        public async Task ScoreAsync()
        {
            var guild = StaticObjects.Db.GetGuild(Context.Guild.Id);
            var embed = new EmbedBuilder
            {
                Title = "Scores",
                Color = Color.Blue
            };
            float globalScore = 0;
            foreach (string s in StaticObjects.AllGameNames)
            {
                StringBuilder str = new StringBuilder();
                if (!guild.DoesContainsGame(s))
                    str.AppendLine("You are not ranked in this game");
                else
                {
                    int myScore = guild.GetScore(s);
                    var scores = StaticObjects.Db.GetAllScores(s);
                    str.AppendLine("You are ranked #" + (scores.Count(x => x > myScore) + 1) + " out of " + scores.Count);
                    str.AppendLine("Your score: " + myScore);
                    var bestScores = scores.Where(x => x > myScore);
                    if (bestScores.Count() > 0)
                        str.AppendLine("Next score for rank up: " + bestScores.Min());
                    str.AppendLine("Best score: " + scores.Max());
                    globalScore += myScore / scores.Max();
                }
                str.AppendLine();
                embed.AddField(s, str.ToString());
            }
            embed.Description = "Global Score: " + (globalScore / StaticObjects.AllGameNames.Length * 100f).ToString("0.00") + "%";
            await ReplyAsync(embed: embed.Build());
        }

        [Command("Play", RunMode = RunMode.Async)]
        public async Task PlayAsync(string gameName, params string[] modes)
        {
            if (StaticObjects.Games.Any(x => x.IsMyGame(Context.Channel.Id)))
                await ReplyAsync("A game is already running in this channel.");
            if (gameName.ToUpperInvariant() == "CUSTOM")
            {
                if (Context.Message.Attachments.Count == 0)
                    throw new CommandFailed("You must attach the file containing game answers");

                var url = Context.Message.Attachments.ToArray()[0].Url;
                using WebClient wc = new WebClient();
                byte[] buffer = wc.DownloadData(url);
                var content = Encoding.Default.GetString(buffer);
                Console.WriteLine(content);
                CustomPreload preload;
                try
                {
                    preload = new CustomPreload(JsonConvert.DeserializeObject<CustomGame>(content));
                } catch (JsonException) {
                    throw new CommandFailed("The JSON given can't be parsed");
                }
                var lobby = modes.Contains("multi") || modes.Contains("multiplayer") ? new MultiplayerLobby(Context.User) : null;
                var game = preload.CreateGame(Context.Channel, Context.User, new GameSettings(lobby, true));
                StaticObjects.Games.Add(game);
                await game.StartWhenReadyAsync();
            }
            else
            {
                var game = LoadGame(gameName.ToLowerInvariant(), Context.Channel, Context.User, modes);
                if (game == null)
                    await ReplyAsync("There is no game with this name.");
                else
                {
                    StaticObjects.Games.Add(game);
                    await game.StartWhenReadyAsync();
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

        [Command("Start"), RequireRunningGame]
        public async Task StartAsync()
        {
            var game = StaticObjects.Games.Find(x => x.IsMyGame(Context.Channel.Id));
            if (!game.IsMultiplayerGame())
                await ReplyAsync("This command can only be done on multiplayer games");
            else if (game.GetState() != GameState.PREPARE)
                await ReplyAsync("The game in this channel is already running.");
            else if (!game.IsLobbyOwner(Context.User))
                await ReplyAsync("Only the lobby owner can use this command");
            else
                await game.StartAsync();
        }

        [Command("Join"), RequireRunningGame]
        public async Task JoinAsync()
        {
            var game = StaticObjects.Games.Find(x => x.IsMyGame(Context.Channel.Id));
            if (!game.IsMultiplayerGame())
                await ReplyAsync("This command can only be done on multiplayer games");
            else if (game.GetState() != GameState.PREPARE)
                await ReplyAsync("The game in this channel is already running.");
            else if (!game.Join(Context.User))
                await ReplyAsync("You are already in the lobby.");
            else
                await ReplyAsync("You joined the lobby.");
        }

        public AGame LoadGame(string gameName, IMessageChannel textChan, IUser user, string[] arguments)
        {
            foreach (var preload in StaticObjects.Preloads)
            {
                if (preload.GetGameNames().Contains(gameName) && (preload.GetNameArg() == null || arguments.Contains(preload.GetNameArg())))
                {
                    if (Context.Channel is ITextChannel chan && !chan.IsNsfw && !preload.IsSafe())
                        throw new CommandFailed("This game can only be launched in a NSFW channel.");
                    var lobby = arguments.Contains("multi") || arguments.Contains("multiplayer") ? new MultiplayerLobby(Context.User) : null;
                    return preload.CreateGame(textChan, user, new GameSettings(lobby, false));
                }
            }
            return null;
        }
    }
}
