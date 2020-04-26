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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SanaraV2.Features.Entertainment
{
    public static class AnimeManga
    {
        private static bool ContainsCheckNull(string s1, string s2)
        {
            if (s1 == null)
                return false;
            return s1.Contains(s2);
        }

        public static async Task<FeatureRequest<Response.Subscribe, Error.Subscribe>> Subscribe(IGuild guild, Db.Db db, string[] args)
        {
            string channel = string.Join(" ", args);
            if (channel.Length == 0)
                return new FeatureRequest<Response.Subscribe, Error.Subscribe>(null, Error.Subscribe.Help);
            ITextChannel chan = await Utilities.GetTextChannelAsync(channel, guild);
            if (chan == null)
                return new FeatureRequest<Response.Subscribe, Error.Subscribe>(null, Error.Subscribe.InvalidChannel);
            await db.AddAnimeSubscription(chan);
            return new FeatureRequest<Response.Subscribe, Error.Subscribe>(new Response.Subscribe
            {
                chan = chan
            }, Error.Subscribe.None);
        }

        public static async Task<FeatureRequest<Response.Unsubscribe, Error.Unsubscribe>> Unsubscribe(IGuild guild, Db.Db db)
        {
            if (!await db.RemoveAnimeSubscription(guild.Id))
                return new FeatureRequest<Response.Unsubscribe, Error.Unsubscribe>(null, Error.Unsubscribe.NoSubscription);
            return new FeatureRequest<Response.Unsubscribe, Error.Unsubscribe>(new Response.Unsubscribe(), Error.Unsubscribe.None);
        }

        private static async Task<dynamic> ContactSource(string url)
        {
            dynamic json;
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    string html = await hc.GetStringAsync("https://trace.moe/api/search?url=" + Uri.EscapeDataString(url));
                    json = JsonConvert.DeserializeObject(html);
                }
            }
            catch (HttpRequestException) // The API return an error 500 on fail
            {
                return null;
            }
            return json;
        }

        private static string AddLeadingZero(string str)
            => str.Length == 1 ? "0" + str : str;

        /// <summary>
        /// Search for an anime/manga using kitsu.io API
        /// </summary>
        /// <param name="isAnime">Is the search an anime (true) or a manga (false)</param>
        /// <param name="args">Name</param>
        /// <returns>Embed containing anime/manga informations (if found)</returns>
        public static async Task<FeatureRequest<Response.AnimeManga, Error.AnimeManga>> SearchAnime(bool isAnime, string[] args, Dictionary<string, string> auth)
        {
            string searchName = Utilities.AddArgs(args);
            if (searchName.Length == 0)
                return (new FeatureRequest<Response.AnimeManga, Error.AnimeManga>(null, Error.AnimeManga.Help));
            string token = null;
            if (auth != null)
            {
                using (HttpClient hc = new HttpClient())
                {
                    var msg = new HttpRequestMessage(HttpMethod.Post, "https://kitsu.io/api/oauth/token");
                    msg.Content = new FormUrlEncodedContent(auth);
                    dynamic j = JsonConvert.DeserializeObject(await (await hc.SendAsync(msg)).Content.ReadAsStringAsync());
                    token = j.access_token;
                }
            }
            dynamic json;
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                json = JsonConvert.DeserializeObject(await (await hc.GetAsync("https://kitsu.io/api/edge/" + ((isAnime) ? ("anime") : ("manga")) + "?page[limit]=5&filter[text]=" + searchName)).Content.ReadAsStringAsync());
            }
            if (json.data.Count == 0)
                return new FeatureRequest<Response.AnimeManga, Error.AnimeManga>(null, Error.AnimeManga.NotFound);
            dynamic finalData = null;
            string cleanUserInput = Utilities.CleanWord(searchName);
            foreach (dynamic data in json.data)
            {
                if (data.attributes.subtype == "TV" &&
                    (ContainsCheckNull(Utilities.CleanWord((string)data.attributes.titles.en), cleanUserInput)
                    || ContainsCheckNull(Utilities.CleanWord((string)data.attributes.titles.en_jp), cleanUserInput)
                    || ContainsCheckNull(Utilities.CleanWord((string)data.attributes.titles.en_us), cleanUserInput)))
                {
                    finalData = data.attributes;
                    break;
                }
            }
            if (finalData == null)
                finalData = json.data[0].attributes;
            return new FeatureRequest<Response.AnimeManga, Error.AnimeManga>(new Response.AnimeManga()
            {
                name = finalData.canonicalTitle,
                imageUrl = finalData.posterImage.original,
                alternativeTitles = finalData.abbreviatedTitles.ToObject<string[]>(),
                episodeCount = finalData.episodeCount,
                episodeLength = finalData.episodeLength,
                rating = finalData.averageRating,
                startDate = finalData.startDate ?? DateTime.ParseExact((string)finalData.startDate, "yyyy-MM-dd", CultureInfo.CurrentCulture),
                endDate = finalData.endDate ?? DateTime.ParseExact((string)finalData.endDate, "yyyy-MM-dd", CultureInfo.CurrentCulture),
                ageRating = finalData.ageRatingGuide,
                synopsis = finalData.synopsis,
                nsfw = finalData.nsfw ?? false,
                animeUrl = "https://kitsu.io/" + ((isAnime) ? ("anime") : ("manga")) + "/" + finalData.slug
            }, Error.AnimeManga.None);
        }
    }
}
