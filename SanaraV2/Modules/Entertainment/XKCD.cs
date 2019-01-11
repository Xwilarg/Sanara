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
using System;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Entertainment
{
    public class Xkcd : ModuleBase
    {
        Program p = Program.p;

        [Command("Xkcd", RunMode = RunMode.Async), Summary("Give XKCD commic")]
        public async Task XkcdSearch(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Xkcd);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Xkcd);
            var result = await Features.Entertainment.Xkcd.SearchXkcd(args, Program.p.rand);
            switch (result.error)
            {
                case Features.Entertainment.Error.Xkcd.InvalidNumber:
                    await ReplyAsync(Sentences.XkcdWrongArg(Context.Guild.Id));
                    break;

                case Features.Entertainment.Error.Xkcd.NotFound:
                    await ReplyAsync(Sentences.XkcdWrongId(Context.Guild.Id, result.answer.maxNb));
                    break;

                case Features.Entertainment.Error.Xkcd.None:
                    await ReplyAsync("", false, new EmbedBuilder() {
                        Title = result.answer.title,
                        Color = Color.Blue,
                        ImageUrl = result.answer.imageUrl,
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = result.answer.alt
                        }
                    }.Build());
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}