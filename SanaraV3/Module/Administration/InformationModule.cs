using Discord;
using Discord.Commands;
using SanaraV3.Help;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadInformationHelp()
        {
            _submoduleHelp.Add("Information", "Get various information about the bot");
            _help.Add(("Administration", new Help("Information", "Help", new[] { new Argument(ArgumentType.MANDATORY, "module/submodule") }, "Display this help.", new string[0], Restriction.None, "Help information")));
            _help.Add(("Administration", new Help("Information", "Status", new Argument[0], "Display various information about the bot.", new string[0], Restriction.None, null)));
            _help.Add(("Administration", new Help("Information", "Premium", new Argument[0], "Get information about premium features.", new string[0], Restriction.None, null)));
        }
    }
}

namespace SanaraV3.Module.Administration
{
    public class InformationModule : ModuleBase
    {
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

        [Command("Help")]
        public async Task Help()
        {
            await ReplyAsync(embed: GetHelpEmbed());
        }

        private Embed GetHelpEmbed()
        {
            Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
            foreach (var help in StaticObjects.Help.GetHelp())
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
                    Text = "Do help module/submodule for more information.\nExample: help information"
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
                Title = name[0] + string.Join("", name.Skip(1).Select(x => char.ToLower(x)))
            };
            if (StaticObjects.Help.GetHelp().Any(x => x.Item2.SubmoduleName.ToUpper() == name))
            {
                StringBuilder str = new StringBuilder();
                Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
                foreach (var help in StaticObjects.Help.GetHelp().Where(x => x.Item2.SubmoduleName.ToUpper() == name))
                {
                    str.AppendLine("**" + help.Item2.CommandName + string.Join(" ", help.Item2.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})")) + $"**: {help.Item2.Description}" +
                        (help.Item2.Example != null ? $"\n*Example: {help.Item2.Example}*" : "") + "\n");
                }
                embed.Description = str.ToString();
            }
            else if (StaticObjects.Help.GetHelp().Any(x => x.Item1.ToUpper() == name))
            {
                Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
                foreach (var help in StaticObjects.Help.GetHelp().Where(x => x.Item1.ToUpper() == name))
                {
                    if (!modules.ContainsKey(help.Item2.SubmoduleName))
                        modules.Add(help.Item2.SubmoduleName, new List<string>());
                    modules[help.Item2.SubmoduleName].Add("**" + help.Item2.CommandName + string.Join(" ", help.Item2.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})")) + $"**: {help.Item2.Description}");
                }
                foreach (var m in modules.OrderBy(x => x.Key))
                {
                    embed.AddField(m.Key, string.Join("\n", m.Value));
                }
            }
            else
                await ReplyAsync(embed: GetHelpEmbed());
            await ReplyAsync(embed: embed.Build());
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
                if (gameNames.Contains(name))
                    continue;
                gameNames.Add(name);
                str.AppendLine($"**{char.ToUpper(name[0]) + string.Join("", name.Skip(1)).ToLower()}**: {elem.Load().Count} words.");
            }
            embed.AddField("Games", str.ToString());
            await ReplyAsync(embed: embed.Build());
        }
    }
}
