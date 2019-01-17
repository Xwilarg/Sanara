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

namespace SanaraV2.Modules.NSFW
{
    public class Doujinshi : ModuleBase
    {
        Program p = Program.p;

        [Command("Doujinshi", RunMode = RunMode.Async), Summary("Give a random doujinshi using nhentai API")]
        public async Task GetNhentai(params string[] keywords)
        {
            Base.Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Doujinshi);
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Doujinshi);
            await ReplyAsync("NHentai API have been suspended indenfinitly by his owner." + Environment.NewLine
                + "This module isn't available for now but will be replaced by e-hentai API soon.");
            return;
            var result = await Features.NSFW.Doujinshi.SearchDoujinshi(!(Context.Channel as ITextChannel).IsNsfw, keywords, Program.p.rand);
            switch (result.error)
            {
                case Features.NSFW.Error.Doujinshi.ChanNotNSFW:
                    await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                    break;

                case Features.NSFW.Error.Doujinshi.NotFound:
                    await ReplyAsync(Base.Sentences.TagsNotFound(keywords));
                    break;

                case Features.NSFW.Error.Doujinshi.None:
                    await ReplyAsync(result.answer.url);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}