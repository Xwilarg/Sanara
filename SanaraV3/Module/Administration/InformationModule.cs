using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Attribute;
using SanaraV3.Exception;
using SanaraV3.Help;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadInformationHelp()
        {
            _submoduleHelp.Add("Information", "Get important information about the bot");
            _help.Add(("Administration", new Help("Information", "Help", new[] { new Argument(ArgumentType.MANDATORY, "module/submodule") }, "Display this help.", new string[0], Restriction.None, "Help information")));
            _help.Add(("Administration", new Help("Information", "Status", new Argument[0], "Display various information about the bot.", new string[0], Restriction.None, null)));
            _help.Add(("Administration", new Help("Information", "Premium", new Argument[0], "Get information about premium features.", new string[0], Restriction.None, null)));
            _help.Add(("Administration", new Help("Information", "V3", new Argument[0], "Get information about the transition from the V2 to the V3.", new string[0], Restriction.None, null)));
            _help.Add(("Administration", new Help("Information", "Logs", new Argument[0], "Get the latest commits made to the bot.", new string[0], Restriction.None, null)));
            _help.Add(("Administration", new Help("Information", "Gdpr", new Argument[0], "Display all the data saved about your guild.", new string[0], Restriction.AdminOnly, null)));
        }
    }
}

namespace SanaraV3.Module.Administration
{
    public class InformationModule : ModuleBase
    {
        [Command("Logs")]
        public async Task LogsAsync()
        {
            if (StaticObjects.GithubKey == null)
                throw new CommandFailed("This command is not available.");

            var embed = new EmbedBuilder
            {
                Title = "Latest changes",
                Url = "https://github.com/Xwilarg/Sanara/commits/master",
                Color = Color.Purple
            };
            var json = JsonConvert.DeserializeObject<JArray>(await StaticObjects.HttpClient.GetStringAsync("https://api.github.com/repos/Xwilarg/Sanara/commits?per_page=5&access_token=" + StaticObjects.GithubKey));
            foreach (var elem in json)
            {
                embed.AddField(DateTime.ParseExact(elem["commit"]["author"]["date"].Value<string>(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy HH:mm:ss") +
                    " by " + elem["commit"]["author"]["name"].Value<string>(), elem["commit"]["message"].Value<string>());
            }
            await ReplyAsync(embed: embed.Build());
        }

        [Command("Premium")]
        public async Task PremiumAsync()
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Premium",
                Color = Color.Blue,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "What is the premium feature?",
                        Value = "While I'm trying to keeping Sanara as open as possible, storage and API calls aren't free\n" +
                            "Therefor some features are now restricted to 'premium' users."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "How can I apply?",
                        Value = "For now users must be manually whitelisted"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Radio module",
                        Value = "YouTube API calls are heavily limited and Radio module need a lot of them, letting everyone use this module would result at reaching the maximum limit of call really quickly"
                    }
                }
            }.Build());
        }

        [Command("V3")]
        public async Task V3Async()
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Sanara V3",
                Color = Color.Blue,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "What is Sanara V3",
                        Value = "Sanara V3 is the current version, released at the end of september of 2020."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "What is the difference with the V2",
                        Value = "The whole code was redesigned.\nFor me, that means implementing new feature is now easier.\nFor you, that means a better bot response time, less crashes and some improvements on current features."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Sanara tells me my feature is not available",
                        Value = "Most of the features from the V2 were reimplemented, however some of them are a bit harder to do.\nI'm planning to add them back but that may take a bit more of time."
                    }
                }
            }.Build());
        }

        [Command("Help")]
        public async Task Help()
        {
            await ReplyAsync(embed: GetHelpEmbed());
        }

        private bool IsNsfw()
            => Context.Channel is ITextChannel chan ? chan.IsNsfw : true;

        private bool IsAdmin()
            => Context.User is IGuildUser user ? Context.Guild.OwnerId == user.Id || user.GuildPermissions.ManageGuild : true;

        private bool IsOwner()
            => Context.User.Id == 144851584478740481; // TODO: Don't hardcode this

        private Embed GetHelpEmbed()
        {
            Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
            foreach (var help in StaticObjects.Help.GetHelp(Context.Guild?.Id ?? 0, IsNsfw(), IsAdmin(), IsOwner()))
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
                        "You might have access to more commands if you are an admin or if you ask in a NSFW channel\n\n" +
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

        [Command("Help")]
        public async Task HelpAsync(string name)
        {
            name = name.ToUpper();
            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Title = name[0] + string.Join("", name.Skip(1).Select(x => char.ToLower(x))),
                Footer = new EmbedFooterBuilder
                {
                    Text = "You might have access to more commands if you are an admin or if you ask in a NSFW channel\n\n" +
                        "[argument]: Mandatory argument\n" +
                        "(argument): Optional argument"
                }
            };
            var fullHelp = StaticObjects.Help.GetHelp(Context.Guild?.Id ?? 0, IsNsfw(), IsAdmin(), IsOwner());
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
            else if (fullHelp.Any(x => x.Item2.CommandName.ToUpper() == name || x.Item2.Aliases.Contains(name)))
            {
                StringBuilder str = new StringBuilder();
                Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
                foreach (var help in fullHelp.Where(x => x.Item2.CommandName.ToUpper() == name || x.Item2.Aliases.Contains(name)))
                {
                    str.AppendLine("**" + help.Item2.CommandName + string.Join(" ", help.Item2.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})")) + $"**: {help.Item2.Description}" +
                        (help.Item2.Example != null ? $"\n*Example: {help.Item2.Example}*" : ""));
                }
                embed.Description = str.ToString();
            }
            else
                throw new CommandFailed("There is no command or module available with this name");
            await ReplyAsync(embed: embed.Build());
        }

        public static Embed GetSingleHelpEmbed(string name, ICommandContext context)
        {
            var fullHelp = StaticObjects.Help.GetHelp(context.Guild?.Id ?? 0, true, true, true);
            name = name.ToUpper();
            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Title = name[0] + string.Join("", name.Skip(1).Select(x => char.ToLower(x))),
                Footer = new EmbedFooterBuilder
                {
                    Text = "This help was displayed because the last command you sent had some invalid argument\n\n" +
                        "[argument]: Mandatory argument\n" +
                        "(argument): Optional argument"
                }
            };
            StringBuilder str = new StringBuilder();
            Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
            foreach (var help in fullHelp.Where(x => x.Item2.CommandName.ToUpper() == name || x.Item2.Aliases.Contains(name)))
            {
                str.AppendLine("**" + help.Item2.CommandName + string.Join(" ", help.Item2.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})")) + $"**: {help.Item2.Description}" +
                    (help.Item2.Example != null ? $"\n*Example: {help.Item2.Example}*" : ""));
            }
            embed.Description = str.ToString();
            return embed.Build();
        }

        [Command("Status")]
        public async Task Status()
        {
            var embed = new EmbedBuilder
            {
                Title = "Status",
                Color = Color.Purple
            };
            embed.AddField("Server count", StaticObjects.Client.Guilds.Count, true);
            embed.AddField("Total user count (may contains duplicate)", StaticObjects.Client.Guilds.Sum(x => x.Users.Count), true);
            StringBuilder str = new StringBuilder();
            List<string> gameNames = new List<string>();
            foreach (var elem in StaticObjects.Preloads)
            {
                string name = elem.GetGameNames()[0];
                if (elem.GetNameArg() != null && elem.GetNameArg() != "hard")
                    continue;
                gameNames.Add(name + (elem.GetNameArg() != null ? $" {elem.GetNameArg()}" : ""));
                var loadInfo = elem.Load();
                if (loadInfo != null)
                    str.AppendLine($"**{char.ToUpper(name[0]) + string.Join("", name.Skip(1)).ToLower()}**: {elem.Load().Count} words.");
                else
                    str.AppendLine($"**{char.ToUpper(name[0]) + string.Join("", name.Skip(1)).ToLower()}**: None");
            }
            embed.AddField("Games", str.ToString());
            embed.AddField("Subscriptions", string.Join("\n", StaticObjects.SM.GetSubscriptionCount().Select(x => "**" + char.ToUpper(x.Key[0]) + string.Join("", x.Key.Skip(1)) + "**: " + x.Value)));
            await ReplyAsync(embed: embed.Build());
        }

        [Command("Gdpr"), RequireAdmin]
        public async Task Gdpr()
        {
            await ReplyAsync("Please check your private messages.");
            await Context.User.SendMessageAsync("```json\n" + (await StaticObjects.Db.DumpAsync(Context.Guild.Id)).Replace("\n", "").Replace("\r", "") + "\n```");
        }
    }
}
