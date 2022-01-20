using Discord;
using Sanara.Exception;
using Sanara.Game;
using Sanara.Help;

namespace Sanara.Module.Command.Impl
{
    public class Game : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Game", "Play various games directly on Discord");
        }

        public CommandInfo[] GetCommands()
        {
            List<ApplicationCommandOptionChoiceProperties> games = new();
            for (int i = 0; i < StaticObjects.Preloads.Length; i++)
            {
                var game = StaticObjects.Preloads[i];
#if !NSFW_BUILD
                if (!game.IsSafe())
                {
                    continue;
                }
#endif
                games.Add(new()
                {
                    Name = (!game.IsSafe() ? "(NSFW) " : "") + game.Name,
                    Value = i
                });
            }

            return new[]
            {
                new CommandInfo(
                   slashCommand: new SlashCommandBuilder()
                   {
                       Name = "play",
                       Description = "Start a game",
                       Options = new()
                       {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "game",
                                Description = "Game you want to play",
                                Type = ApplicationCommandOptionType.Integer,
                                IsRequired = true,
                                Choices = games
                            }
                       }
                   }.Build(),
                   callback: PlayAsync,
                   precondition: Precondition.None,
                   needDefer: false
               ),
               new CommandInfo(
                   slashCommand: new SlashCommandBuilder()
                   {
                       Name = "cancel",
                       Description = "Stop the current game"
                   }.Build(),
                   callback: CancelAsync,
                   precondition: Precondition.None,
                   needDefer: false
               )
            };
        }

        public async Task PlayAsync(ICommandContext ctx)
        {
            if (StaticObjects.GameManager.IsChannelBusy(ctx.Channel))
                throw new CommandFailed("A game is already running in this channel.");
            /*if (gameName.ToUpperInvariant() == "CUSTOM")
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
                }
                catch (JsonException)
                {
                    throw new CommandFailed("The JSON given can't be parsed");
                }
                var lobby = modes.Contains("multi") || modes.Contains("multiplayer") ? new MultiplayerLobby(Context.User) : null;
                var game = preload.CreateGame(Context.Channel, Context.User, new GameSettings(lobby, true));
                StaticObjects.Games.Add(game);
                await game.StartWhenReadyAsync();
            }
            else*/
            {
                var index = ctx.GetArgument<long>("game");
                var preload = StaticObjects.Preloads[(int)index];
                if (ctx.Channel is ITextChannel chan && !chan.IsNsfw && !preload.IsSafe())
                    throw new CommandFailed("This game can only be launched in a NSFW channel.");
                var lobby = new Lobby(ctx.User, preload);
                var game = preload.CreateGame(ctx.Channel, ctx.User, new GameSettings(lobby, false));

                await StaticObjects.GameManager.CreateGameAsync(ctx.Channel, game);

                var buttons = new ComponentBuilder()
                    .WithButton(label: "Start", customId: $"game/start", style: ButtonStyle.Success)
                    .WithButton(label: "Join/Leave", customId: $"game/join")
                    .WithButton(label: "Switch multiplayer mode", customId: $"game/multi")
                    .WithButton(label: "Cancel", customId: $"game/cancel", style: ButtonStyle.Danger);

                await ctx.ReplyAsync(embed: lobby.GetIntroEmbed(), components: buttons.Build());
            }
        }

        public async Task CancelAsync(ICommandContext ctx)
        {
            var game = StaticObjects.GameManager.GetGame(ctx.Channel);

            if (game == null)
            {
                throw new CommandFailed("There is no game running in this channel");
            }
            await game.CancelAsync();
        }
    }
}
/*
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
    }
}
*/