﻿using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV3.Module.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadInformationHelp()
        {
            _help.Add(new Help("Help", new Argument[0], "Display this help.", false));
            _help.Add(new Help("Status", new Argument[0], "Display various information about the bot.", false));
            _help.Add(new Help("Premium", new Argument[0], "Get information about premium features.", false));
        }
    }

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
            StringBuilder str = new StringBuilder();
            foreach (var help in StaticObjects.Help.GetHelp())
            {
                if (!help.IsNsfw || !(Context.Channel is ITextChannel) || ((ITextChannel)Context.Channel).IsNsfw)
                    str.AppendLine($"**{help.CommandName} {string.Join(" ", help.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})"))}**: {help.Description}");
            }
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                Title = "Help",
                Description = str.ToString()
            }.Build());
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
