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

namespace SanaraV2.Modules.Entertainment
{
    public class AnimeManga : ModuleBase
    {
        Program p = Program.p;

        [Command("Anime", RunMode = RunMode.Async), Summary("Give informations about an anime using MyAnimeList API")]
        public async Task Mal(params string[] animeNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            var result = await Features.Entertainment.AnimeManga.SearchAnime(true, animeNameArr);
            switch (result.error)
            {
                case Features.Entertainment.Error.AnimeMangaError.Help:
                    await ReplyAsync(Sentences.AnimeHelp(Context.Guild.Id));
                    break;

                case Features.Entertainment.Error.AnimeMangaError.NotFound:
                    await ReplyAsync(Sentences.AnimeNotFound(Context.Guild.Id));
                    break;

                case Features.Entertainment.Error.AnimeMangaError.None:
                    await ReplyAsync("", false, result.answer);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Manga", RunMode = RunMode.Async), Summary("Give informations about a manga using MyAnimeList API")]
        public async Task MalManga(params string[] mangaNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            var result = await Features.Entertainment.AnimeManga.SearchAnime(false, mangaNameArr);
            switch (result.error)
            {
                case Features.Entertainment.Error.AnimeMangaError.Help:
                    await ReplyAsync(Sentences.MangaHelp(Context.Guild.Id));
                    break;

                case Features.Entertainment.Error.AnimeMangaError.NotFound:
                    await ReplyAsync(Sentences.MangaNotFound(Context.Guild.Id));
                    break;

                case Features.Entertainment.Error.AnimeMangaError.None:
                    await ReplyAsync("", false, result.answer);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}