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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

namespace SanaraV2.Modules.Entertainment
{
    public class VnModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Vn", RunMode = RunMode.Async)]
        public async Task Vndb(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Vn);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Vn);
            var result = await Features.Entertainment.Vn.SearchVn(args, !((ITextChannel)Context.Channel).IsNsfw);
            switch (result.error)
            {
                case Error.Vn.Help:
                    await ReplyAsync(Sentences.VndbHelp(Context.Guild.Id));
                    break;

                case Error.Vn.NotFound:
                    await ReplyAsync(Sentences.VndbNotFound(Context.Guild.Id));
                    break;

                case Error.Vn.None:
                    await ReplyAsync("", false, new EmbedBuilder()
                    {
                        Title = result.answer.originalTitle == null ? result.answer.title : result.answer.originalTitle + " (" + result.answer.title + ")",
                        ImageUrl = result.answer.imageUrl,
                        Description = result.answer.description,
                        Color = Color.Blue
                    }.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}