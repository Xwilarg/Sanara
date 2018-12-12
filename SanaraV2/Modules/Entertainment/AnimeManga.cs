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
using System;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Entertainment
{
    public class AnimeManga : ModuleBase
    {
        Program p = Program.p;

        [Command("Anime", RunMode = RunMode.Async), Summary("Give informations about an anime using MyAnimeList API")]
        public async Task Anime(params string[] animeNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            var result = await Features.Entertainment.AnimeManga.SearchAnime(true, animeNameArr);
            switch (result.error)
            {
                case Error.AnimeManga.Help:
                    await ReplyAsync(Sentences.AnimeHelp(Context.Guild.Id));
                    break;

                case Error.AnimeManga.NotFound:
                    await ReplyAsync(Sentences.AnimeNotFound(Context.Guild.Id));
                    break;

                case Error.AnimeManga.None:
                    await ReplyAsync("", false, CreateEmbed(true, result.answer, Context.Guild.Id));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [Command("Manga", RunMode = RunMode.Async), Summary("Give informations about a manga using MyAnimeList API")]
        public async Task Manga(params string[] mangaNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.AnimeManga);
            var result = await Features.Entertainment.AnimeManga.SearchAnime(false, mangaNameArr);
            switch (result.error)
            {
                case Error.AnimeManga.Help:
                    await ReplyAsync(Sentences.MangaHelp(Context.Guild.Id));
                    break;

                case Error.AnimeManga.NotFound:
                    await ReplyAsync(Sentences.MangaNotFound(Context.Guild.Id));
                    break;

                case Error.AnimeManga.None:
                    await ReplyAsync("", false, CreateEmbed(false, result.answer, Context.Guild.Id));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private Embed CreateEmbed(bool isAnime, Response.AnimeManga res, ulong guildId)
        {
            return (new EmbedBuilder
            {
                Title = res.name,
                Color = Color.Green,
                ImageUrl = res.imageUrl,
                Description = ((res.alternativeTitles.Length > 0) ? (Base.Utilities.CapitalizeFirstLetter(Base.Sentences.OrStr(guildId)) + " " + string.Join(", ", res.alternativeTitles)
                 + Environment.NewLine + Environment.NewLine) : ("")) + ((isAnime && res.episodeCount != null) ? (Sentences.AnimeEpisodes(guildId, res.episodeCount.Value) + ((res.episodeLength != null) ? (" " + Sentences.AnimeLength(guildId, res.episodeLength.Value)) : ("")) + Environment.NewLine) : (""))
                 + Sentences.AnimeRating(guildId, res.rating) + Environment.NewLine
                 + ((res.startDate != null) ? Sentences.AnimeDate(guildId, res.startDate.Value.ToString(Base.Sentences.DateHourFormatShort(guildId)), ((res.endDate != null) ? (res.endDate.Value.ToString(Base.Sentences.DateHourFormatShort(guildId))) : (Sentences.Unknown(guildId)))) : (Sentences.ToBeAnnounced(guildId))) + Environment.NewLine
                 + ((string.IsNullOrEmpty(res.ageRating)) ? ("") : (Sentences.AnimeAudiance(guildId, res.ageRating)))
                 + Environment.NewLine + Environment.NewLine + res.synopsis
            }.Build());
        }
    }
}