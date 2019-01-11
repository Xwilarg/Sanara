/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Discord;
using System.Linq;

namespace SanaraV2.Modules.Tools
{
    public class Linguist : ModuleBase
    {
        Program p = Program.p;

        [Command("Urban", RunMode = RunMode.Async), Summary("Search for a word in Urban Dictionary")]
        public async Task Urban(params string[] words)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Linguistic);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            var result = await Features.Tools.Linguist.UrbanSearch(!((ITextChannel)Context.Channel).IsNsfw, words);
            switch (result.error)
            {
                case Features.Tools.Error.Urban.Help:
                    await ReplyAsync(Sentences.UrbanHelp(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Urban.ChanNotNSFW:
                    await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Urban.NotFound:
                    await ReplyAsync(Sentences.UrbanNotFound(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Urban.None:
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Color = Color.Blue,
                        Title = char.ToUpper(result.answer.word[0]) + string.Concat(result.answer.word.Skip(1)),
                        Fields = new System.Collections.Generic.List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = Sentences.Definition(Context.Guild.Id),
                                Value = result.answer.definition
                            },
                            new EmbedFieldBuilder()
                            {
                                Name = Sentences.Example(Context.Guild.Id),
                                Value = result.answer.example
                            }
                        },
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = Base.Sentences.FromStr(Context.Guild.Id, result.answer.link)
                        }
                    }.Build());
                    break;
            }
        }

        [Command("Translation", RunMode = RunMode.Async), Summary("Translate a sentence"), Alias("Translate")]
        public async Task Translation(params string[] words)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Linguistic);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            var result = await Features.Tools.Linguist.Translate(words, Program.p.translationClient, Program.p.visionClient, Program.p.allLanguages);
            switch (result.error)
            {
                case Features.Tools.Error.Translation.Help:
                    await ReplyAsync(Sentences.TranslateHelp(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Translation.InvalidApiKey:
                    await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Translation.InvalidLanguage:
                    await ReplyAsync(Sentences.InvalidLanguage(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Translation.NotAnImage:
                    await ReplyAsync(Sentences.NotAnImage(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Translation.NoTextOnImage:
                    await ReplyAsync(Sentences.NoTextOnImage(Context.Guild.Id));
                    break;

                case Features.Tools.Error.Translation.None:
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Color = Color.Blue,
                        Title = Base.Sentences.FromStr(Context.Guild.Id, result.answer.sourceLanguage),
                        Description = result.answer.sentence
                    }.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Japanese", RunMode = RunMode.Async), Summary("Give the meaning of a word")]
        public async Task Meaning(params string[] words)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Linguistic);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            var result = await Features.Tools.Linguist.JapaneseTranslate(words);
            switch (result.error)
            {
                case Features.Tools.Error.JapaneseTranslation.Help:
                    await ReplyAsync(Sentences.JapaneseHelp(Context.Guild.Id));
                    break;

                case Features.Tools.Error.JapaneseTranslation.NotFound:
                    await ReplyAsync(Sentences.NoJapaneseTranslation(Context.Guild.Id));
                    break;

                case Features.Tools.Error.JapaneseTranslation.None:
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = Color.Blue,
                        Title = string.Join(" ", words)
                    };
                    int i = 0;
                    foreach (var answer in result.answer)
                    {
                        embed.AddField(string.Join(", ", answer.definition),
                            string.Join(Environment.NewLine, answer.words.Select((Features.Tools.Response.JapaneseWord word) =>
                            ((word.word != null) ? (word.word + " - ") : ("")) + ((word.reading != null) ? (word.reading + " (" + word.romaji + ")") : ("")))));
                        if (++i == 5)
                            break;
                    }
                    await ReplyAsync("", false, embed.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}