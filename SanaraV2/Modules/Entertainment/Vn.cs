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
using Discord;
using Discord.Commands;
using SanaraV2.Features.Entertainment;
using SanaraV2.Modules.Base;
using System;
using System.Threading.Tasks;
using VndbSharp.Models.VisualNovel;

namespace SanaraV2.Modules.Entertainment
{
    public class VnModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Vn", RunMode = RunMode.Async)]
        public async Task Vndb(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild, Program.Module.Vn);
            await p.DoAction(Context.User, Program.Module.Vn);
            var result = await Vn.SearchVn(args, !((ITextChannel)Context.Channel).IsNsfw);
            switch (result.error)
            {
                case Error.Vn.Help:
                    await ReplyAsync(Sentences.VndbHelp(Context.Guild));
                    break;

                case Error.Vn.NotFound:
                    await ReplyAsync(Sentences.VndbNotFound(Context.Guild));
                    break;

                case Error.Vn.None:
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Title = result.answer.originalTitle == null ? result.answer.title : result.answer.originalTitle + " (" + result.answer.title + ")",
                        Url = result.answer.vnUrl,
                        ImageUrl = result.answer.imageUrl,
                        Description = result.answer.description,
                        Color = Color.Blue
                    };
                    embed.AddField(Sentences.AvailableEnglish(Context.Guild), result.answer.isAvailableEnglish ? Base.Sentences.YesStr(Context.Guild) : Base.Sentences.NoStr(Context.Guild), true);
                    embed.AddField(Sentences.AvailableWindows(Context.Guild), result.answer.isAvailableWindows ? Base.Sentences.YesStr(Context.Guild) : Base.Sentences.NoStr(Context.Guild), true);
                    string length = Sentences.Unknown(Context.Guild);
                    switch (result.answer.length)
                    {
                        case VisualNovelLength.VeryShort: length = Sentences.Hours(Context.Guild, "< 2 "); break;
                        case VisualNovelLength.Short: length = Sentences.Hours(Context.Guild, "2 - 10 "); break;
                        case VisualNovelLength.Medium: length = Sentences.Hours(Context.Guild, "10 - 30 "); break;
                        case VisualNovelLength.Long: length = Sentences.Hours(Context.Guild, "30 - 50 "); break;
                        case VisualNovelLength.VeryLong: length = Sentences.Hours(Context.Guild, "> 50 "); break;
                    }
                    embed.AddField(Sentences.Length(Context.Guild), length, true);
                    embed.AddField(Sentences.VndbRating(Context.Guild), result.answer.rating + " / 10", true);
                    string releaseDate;
                    if (result.answer.releaseYear == null)
                        releaseDate = Sentences.Tba(Context.Guild);
                    else
                    {
                        releaseDate = result.answer.releaseYear.ToString();
                        if (result.answer.releaseMonth != null)
                            releaseDate = Utilities.AddZero(result.answer.releaseMonth.ToString()) + "/" + releaseDate;
                        if (result.answer.releaseDay != null)
                            releaseDate = Utilities.AddZero(result.answer.releaseDay.ToString()) + "/" + releaseDate;
                    }
                    embed.AddField(Sentences.ReleaseDate(Context.Guild), releaseDate, true);
                    await ReplyAsync("", false, embed.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}