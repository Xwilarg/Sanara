using Discord;
using Discord.Commands;
using SanaraV2.Modules.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV2.Modules.GamesInfo
{
    [Group("Arknights"), Alias("Ak")]
    public class Arknights : ModuleBase
    {
        [Command("", RunMode = RunMode.Async), Priority(-1)]
        public async Task CharacDefault(params string[] shipNameArr) => await Charac(shipNameArr);

        [Command("Charac", RunMode = RunMode.Async), Alias("Character")]
        public async Task Charac(params string[] name)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Arknights);
            await Program.p.DoAction(Context.User, Program.Module.Arknights);
            var result = await Features.GamesInfo.Arknights.SearchCharac(name);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Charac.Help:
                    await ReplyAsync(Sentences.ArknightsHelp(Context.Guild));
                    break;

                case Features.GamesInfo.Error.Charac.NotFound:
                    await ReplyAsync(Sentences.OperatorDontExist(Context.Guild));
                    break;

                case Features.GamesInfo.Error.Charac.InvalidLevel:
                    await ReplyAsync("The level given must be between 1 and the maximum skill level.");
                    break;

                case Features.GamesInfo.Error.Charac.None:
                    List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>
                    {
                            new EmbedFieldBuilder
                            {
                                Name = "Position",
                                Value = result.answer.type,
                                IsInline = true
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "HR Tags",
                                Value = string.Join(", ", result.answer.tags),
                                IsInline = true
                            }
                    };
                    foreach (var skill in result.answer.skills)
                    {
                        fields.Add(new EmbedFieldBuilder
                        {
                            Name = skill.name,
                            Value = skill.description
                        });
                    }
                    EmbedFooterBuilder footer = null;
                    if (result.answer.skills.Length > 0)
                    {
                        footer = new EmbedFooterBuilder
                        {
                            Text = "Skills displayed are at " + (result.answer.skillLevel < 8 ? "level " + result.answer.skillLevel : "mastery " + (result.answer.skillLevel - 7))
                        };
                    }
                    var msg = await ReplyAsync("", false, new EmbedBuilder
                    {
                        Title = result.answer.name,
                        Url = result.answer.wikiUrl,
                        Description = result.answer.description,
                        ImageUrl = result.answer.imgUrl,
                        Color = Color.Blue,
                        Fields = fields,
                        Footer = footer
                    }.Build());
                    if (result.answer.skills.Length > 0)
                    {
                        await msg.AddReactionsAsync(new[] { new Emoji("⏪"), new Emoji("◀️"), new Emoji("▶️"), new Emoji("⏩") });
                        Program.p.ARKNIGHTS_SKILLS_INFO.Add(msg.Id, (result.answer.skillLevel - 1, result.answer.skillKeys));
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
