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
                json = JsonConvert.DeserializeObject(await(await hc.GetAsync("https://kitsu.io/api/edge/" + ((isAnime) ? ("anime") : ("manga")) + "?page[limit]=5&filter[text]=" + searchName)).Content.ReadAsStringAsync());
            if (json.data.Count == 0)
                return (new FeatureRequest<Response.AnimeManga, Error.AnimeManga>(null, Error.AnimeManga.NotFound));
            dynamic finalData = null;
            foreach (dynamic data in json.data)
            {
                if (data.attributes.subtype == "TV")
                {
                    finalData = data.attributes;
                    break;
                }
            }
            if (finalData == null)
                finalData = json.data[0].attributes;
            return (new FeatureRequest<Response.AnimeManga, Error.AnimeManga>(new Response.AnimeManga()
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
                synopsis = finalData.synopsis
            }, Error.AnimeManga.None));
        }
    }
}
