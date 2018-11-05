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
using Newtonsoft.Json;
using SanaraV2.Base;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Entertainment
{
    public class AnimeMangaModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Anime", RunMode = RunMode.Async), Summary("Give informations about an anime using MyAnimeList API")]
        public async Task Mal(params string[] animeNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            await SearchAnime(false, animeNameArr, Context.Channel as ITextChannel);
        }

        [Command("Manga", RunMode = RunMode.Async), Summary("Give informations about a manga using MyAnimeList API")]
        public async Task MalManga(params string[] mangaNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            await SearchAnime(false, mangaNameArr, Context.Channel as ITextChannel);
        }

        private async Task SearchAnime(bool isAnime, string[] args, ITextChannel chan)
        {
            string searchName = Utilities.AddArgs(args);
            if (searchName.Length == 0)
            {
                if (isAnime)
                    await chan.SendMessageAsync(Sentences.AnimeHelp(chan.Id));
                else
                    await chan.SendMessageAsync(Sentences.MangaHelp(chan.Id));
                return;
            }
            dynamic json;
            using (HttpClient hc = new HttpClient())
                json = JsonConvert.DeserializeObject(await (await hc.GetAsync("https://kitsu.io/api/edge/" + ((isAnime) ? ("anime") : ("manga")) + "?page[limit]=1&filter[text]=" + searchName)).Content.ReadAsStringAsync());
            if (json.data.Count == 0)
            {
                if (isAnime)
                    await chan.SendMessageAsync(Sentences.AnimeNotFound(chan.Id));
                else
                    await chan.SendMessageAsync(Sentences.MangaNotFound(chan.Id));
                return;
            }
            dynamic data = json.data[0].attributes;
            await chan.SendMessageAsync("", false, new EmbedBuilder
            {
                Title = data.canonicalTitle,
                Color = Color.Green,
                ImageUrl = data.posterImage.original,
                Description = ((data.abbreviatedTitles.Count > 0) ? ("Or " + string.Join(", ", data.abbreviatedTitles) + Environment.NewLine + Environment.NewLine) : ("")) + "Average rating: " + data.averageRating + Environment.NewLine + Environment.NewLine + data.synopsis
            }.Build());
        }
    }
}