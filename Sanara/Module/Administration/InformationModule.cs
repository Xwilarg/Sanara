using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Help;
using System.Globalization;
using System.Text;

namespace Sanara.Module.Administration
{
    public class InformationModule : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Information", "Get important information about the bot");
            /*
            _help.Add(("Administration", new Help("Information", "Help", new[] { new Argument(ArgumentType.Mandatory, "module/submodule") }, "Display this help.", Array.Empty<string>(), Restriction.None, "Help information")));
            _help.Add(("Administration", new Help("Information", "Status", Array.Empty<Argument>(), "Display various information about the bot.", Array.Empty<string>(), Restriction.None, null)));
            _help.Add(("Administration", new Help("Information", "Logs", Array.Empty<Argument>(), ".", Array.Empty<string>(), Restriction.None, null)));
            _help.Add(("Administration", new Help("Information", "Gdpr", Array.Empty<Argument>(), "Display all the data saved about your guild.", Array.Empty<string>(), Restriction.AdminOnly, null)));
            _help.Add(("Administration", );
            */
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "ping",
                        Description = "Get the latency between the bot and Discord"
                    }.Build(),
                    callback: PingAsync
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "status",
                        Description = "Get various information about the bot"
                    }.Build(),
                    callback: StatusAsync
                )
            };
        }

        public async Task PingAsync(SocketSlashCommand ctx)
        {
            var content = ":ping_pong: Pong!";
            await ctx.RespondAsync(content);
            var orMsg = await ctx.GetOriginalResponseAsync();
            await ctx.ModifyOriginalResponseAsync(x => x.Content = orMsg.Content + "\nLatency: " + orMsg.CreatedAt.Subtract(ctx.CreatedAt).TotalMilliseconds + "ms");
        }

        public async Task StatusAsync(SocketSlashCommand ctx)
        {
            var embed = new EmbedBuilder
            {
                Title = "Status",
                Color = Color.Purple
            };
            embed.AddField("Guild count", StaticObjects.Client.Guilds.Count, true);
            embed.AddField("Total user count (may contains duplicate)", StaticObjects.Client.Guilds.Sum(x => x.Users.Count), true);
            StringBuilder str = new();
            List<string> gameNames = new();
            foreach (var elem in StaticObjects.Preloads)
            {
                string name = elem.GetGameNames()[0];
                if (elem.GetNameArg() != null && elem.GetNameArg() != "hard")
                    continue;
                var fullName = name + (elem.GetNameArg() != null ? $" {elem.GetNameArg()}" : "");
                var loadInfo = elem.Load();
                if (loadInfo != null)
                    str.AppendLine($"**{char.ToUpper(fullName[0]) + string.Join("", fullName.Skip(1)).ToLower()}**: {elem.Load().Count} words.");
                else
                    str.AppendLine($"**{char.ToUpper(fullName[0]) + string.Join("", fullName.Skip(1)).ToLower()}**: None");
            }
            embed.AddField("Games", str.ToString());
            var subs = StaticObjects.GetSubscriptionCount();
            embed.AddField("Subscriptions",
                subs == null ?
                    "Not yet initialized" :
#if NSFW_BUILD
                    string.Join("\n", subs.Select(x => "**" + char.ToUpper(x.Key[0]) + string.Join("", x.Key.Skip(1)) + "**: " + x.Value)));
#else
                    "**Anime**: " + subs["anime"]);
#endif
            /*var json = JsonConvert.DeserializeObject<JArray>(await StaticObjects.HttpClient.GetStringAsync("https://api.github.com/repos/Xwilarg/Sanara/commits?per_page=5"));
            foreach (var elem in json)
            {
                embed.AddField(DateTime.ParseExact(elem["commit"]["author"]["date"].Value<string>(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm:ss") +
                    " by " + elem["commit"]["author"]["name"].Value<string>(), elem["commit"]["message"].Value<string>());
            }
            embed.AddField("Latest changes", json)*/
            await ctx.RespondAsync(embed: embed.Build());
        }
        /*

        [Command("Help")]
        public async Task Help()
        {
            await ReplyAsync(embed: await GetHelpEmbedAsync());
        }

        private bool IsNsfw()
            => Context.Channel is ITextChannel chan ? chan.IsNsfw : true;

        private bool IsAdmin()
            => Context.User is IGuildUser user ? Context.Guild.OwnerId == user.Id || user.GuildPermissions.ManageGuild : true;

        private async Task<bool> IsOwnerAsync()
        {
            if (_ownerId == 0)
            {
                _ownerId = (await StaticObjects.Client.GetApplicationInfoAsync()).Owner.Id;
            }
            return Context.User.Id == _ownerId;
        }

        private static ulong _ownerId = 0;

        private async Task<Embed> GetHelpEmbedAsync()
        {
            Dictionary<string, List<string>> modules = new();
            foreach (var help in StaticObjects.Help.GetHelp(Context.Guild?.Id ?? 0, IsNsfw(), IsAdmin(), await IsOwnerAsync()))
            {
                if (!modules.ContainsKey(help.Item1))
                    modules.Add(help.Item1, new List<string>());
                if (!modules[help.Item1].Contains(help.Item2.SubmoduleName))
                    modules[help.Item1].Add(help.Item2.SubmoduleName);
            }
            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Title = "Help",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Do help module/submodule for more information.\nExample: help information\n\n" +
#if NSFW_BUILD
                        "You might have access to more commands if you are an admin or if you ask in a NSFW channel\n\n" +
#endif
                        "[argument]: Mandatory argument\n" +
                        "(argument): Optional argument"
                }
            };
            foreach (var m in modules.OrderBy(x => x.Key))
            {
                embed.AddField(m.Key, string.Join("\n", m.Value.Select(x => "**" + x + "** - " + StaticObjects.Help.GetSubmoduleHelp(x))));
            }
            return embed.Build();
        }
        /*
        [Command("Help")]
        public async Task HelpAsync(string name)
        {
            name = name.ToUpper();
            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Title = "Help",
                Footer = new EmbedFooterBuilder
                {
                    Text =
#if NSFW_BUILD
                        "You might have access to more commands if you are an admin or if you ask in a NSFW channel\n\n" +
#endif
                        "[argument]: Mandatory argument\n" +
                        "(argument): Optional argument"
                }
            };
            var fullHelp = StaticObjects.Help.GetHelp(Context.Guild?.Id ?? 0, IsNsfw(), IsAdmin(), await IsOwnerAsync());
            if (fullHelp.Any(x => x.Item2.SubmoduleName.ToUpper() == name))
            {
                StringBuilder str = new StringBuilder();
                Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
                foreach (var help in fullHelp.Where(x => x.Item2.SubmoduleName.ToUpper() == name))
                {
                    str.AppendLine("**" + help.Item2.CommandName + " " + string.Join(" ", help.Item2.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})")) + $"**: {help.Item2.Description}" +
                        (help.Item2.Example != null ? $"\n*Example: {help.Item2.Example}*" : "") + "\n");
                }
                embed.Description = str.ToString();
            }
            else if (fullHelp.Any(x => x.Item1.ToUpper() == name))
            {
                Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
                foreach (var help in fullHelp.Where(x => x.Item1.ToUpper() == name))
                {
                    if (!modules.ContainsKey(help.Item2.SubmoduleName))
                        modules.Add(help.Item2.SubmoduleName, new List<string>());
                    modules[help.Item2.SubmoduleName].Add("**" + help.Item2.CommandName + " " + string.Join(" ", help.Item2.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})")) + $"**: {help.Item2.Description}");
                }
                foreach (var m in modules.OrderBy(x => x.Key))
                {
                    embed.AddField(m.Key, string.Join("\n", m.Value));
                }
            }
            else if (fullHelp.Any(x => name.Contains(x.Item2.CommandName.ToUpper()) || x.Item2.Aliases.Any(x => name.Contains(x))))
            {
                StringBuilder str = new StringBuilder();
                Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
                foreach (var help in fullHelp.Where(x => name.Contains(x.Item2.CommandName.ToUpper()) || x.Item2.Aliases.Any(x => name.Contains(x))))
                {
                    str.AppendLine("**" + help.Item2.CommandName + " " + string.Join(" ", help.Item2.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})")) + $"**: {help.Item2.Description}" +
                        (help.Item2.Example != null ? $"\n*Example: {help.Item2.Example}*" : ""));
                }
                embed.Description = str.ToString();
            }
            else
                throw new CommandFailed("There is no command or module available with this name");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("Gdpr"), RequireAdmin]
        public async Task Gdpr()
        {
            await ReplyAsync("Please check your private messages.");
            await Context.User.SendMessageAsync("```json\n" + (await StaticObjects.Db.DumpAsync(Context.Guild.Id)).Replace("\n", "").Replace("\r", "") + "\n```");
        }*/
    }
}
