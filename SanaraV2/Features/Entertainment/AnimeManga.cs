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
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Features.Entertainment
{
    public static class AnimeManga
    {
        /// <summary>
        /// Search for an anime/manga using kitsu.io API
        /// </summary>
        /// <param name="isAnime">Is the search an anime (true) or a manga (false)</param>
        /// <param name="args">Name</param>
        /// <returns>Embed containing anime/manga informations (if found)</returns>
        public static async Task<FeatureRequest<Response.AnimeManga, Error.AnimeManga>> SearchAnime(bool isAnime, string[] args)
        {
            string searchName = Utilities.AddArgs(args);
            if (searchName.Length == 0)
                return (new FeatureRequest<Response.AnimeManga, Error.AnimeManga>(null, Error.AnimeManga.Help));
            dynamic json;
            using (HttpClient hc = new HttpClient())
                json = JsonConvert.DeserializeObject(await(await hc.GetAsync("https://kitsu.io/api/edge/" + ((isAnime) ? ("anime") : ("manga")) + "?page[limit]=1&filter[text]=" + searchName)).Content.ReadAsStringAsync());
            if (json.data.Count == 0)
                return (new FeatureRequest<Response.AnimeManga, Error.AnimeManga>(null, Error.AnimeManga.NotFound));
            dynamic data = json.data[0].attributes;
            return (new FeatureRequest<Response.AnimeManga, Error.AnimeManga>(new Response.AnimeManga()
            {
                name = data.canonicalTitle,
                imageUrl = data.posterImage.original,
                alternativeTitles = data.abbreviatedTitles.ToObject<string[]>(),
                episodeCount = data.episodeCount,
                episodeLength = data.episodeLength,
                rating = data.averageRating,
                startDate = data.startDate ?? DateTime.ParseExact((string)data.startDate, "yyyy-MM-dd", CultureInfo.CurrentCulture),
                endDate = data.endDate ?? DateTime.ParseExact((string)data.endDate, "yyyy-MM-dd", CultureInfo.CurrentCulture),
                ageRating = data.ageRatingGuide,
                synopsis = data.synopsis
            }, Error.AnimeManga.None));
        }
    }
}
