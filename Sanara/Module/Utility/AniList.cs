using Newtonsoft.Json;
using Quickenshtein;
using System.Text;

namespace Sanara.Module.Utility
{
    public static class AniList
    {
        public static async Task<AnimeResult?> SearchMediaAsync(JapaneseMedia media, string query, bool onlyExactMatch = false)
        {
            var json = JsonConvert.SerializeObject(new GraphQL
            {
                query = "query ($search: String) { Page(perPage: 10) { media(type: " + (media == JapaneseMedia.Anime ? "ANIME" : "MANGA") + ", search: $search) { id title { romaji english native } isAdult description(asHtml: false) coverImage { large } averageScore episodes duration startDate { year month day } endDate { year month day } source(version: 3) format type tags { name rank } genres } } }",
                variables = new Dictionary<string, dynamic>
                {
                    { "search", query }
                }
            });

            var answer = await StaticObjects.HttpClient.PostAsync("https://graphql.anilist.co", new StringContent(json, Encoding.UTF8, "application/json"));
            answer.EnsureSuccessStatusCode();

            var str = await answer.Content.ReadAsStringAsync();

            var results = JsonConvert.DeserializeObject<AnimeInfo>(str);

            var target = results.data.Page.media;
            var search = query.ToUpperInvariant();
            if (onlyExactMatch)
            {
                return target.FirstOrDefault(x =>
                 search == (x.title.english?.ToUpperInvariant() ?? "") ||
                 search == (x.title.native?.ToUpperInvariant() ?? "") ||
                 search == (x.title.romaji?.ToUpperInvariant() ?? ""));
            }

            if (media == JapaneseMedia.LightNovel)
            {
                target = target.Where(x => x.format == "NOVEL").ToArray();
            }
            else if (media == JapaneseMedia.Manga)
            {
                target = target.Where(x => x.format != "NOVEL").ToArray();
            }

            if (!target.Any())
            {
                return null;
            }

            var ordered = target.Select(x => // Get the closest title to what we are looking for
                (x, Math.Min(Math.Min(Levenshtein.GetDistance(search, x.title.english?.ToUpperInvariant() ?? ""),
                    Levenshtein.GetDistance(search, x.title.romaji?.ToUpperInvariant() ?? "")),
                        Levenshtein.GetDistance(search, x.title.native?.ToUpperInvariant() ?? "")))).OrderBy(x => x.Item2).ToArray();

            var smallest = ordered.Where(x => x.Item2 == ordered[0].Item2); // Get all the items that have the smallest scores
            if (smallest.Count() == 1) // There is only one so we pick this one
            {
                return smallest.First().x;
            }

            var tv = ordered.Where(x => x.x.format == "TV"); // Else we try to get the one that is on "TV" format
            if (tv.Any())
            {
                return tv.First().x;
            }

            return ordered.First().x;
        }

        public static async Task<AiringSchedule[]> GetAnimeFeedAsync()
        {
            var json = JsonConvert.SerializeObject(new GraphQL
            {
                query = "query { Page(perPage: 25) { airingSchedules(notYetAired: true) { id airingAt episode media { id title { romaji } description(asHtml: false) coverImage { large } isAdult } } } }",
                variables = new Dictionary<string, dynamic>()
            });

            var answer = await StaticObjects.HttpClient.PostAsync("https://graphql.anilist.co", new StringContent(json, Encoding.UTF8, "application/json"));
            answer.EnsureSuccessStatusCode();

            var str = await answer.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<AnimeInfo>(str).data.Page.airingSchedules;
        }

        private class GraphQL
        {
            public string query;
            public Dictionary<string, dynamic> variables;
        }
    }
}
