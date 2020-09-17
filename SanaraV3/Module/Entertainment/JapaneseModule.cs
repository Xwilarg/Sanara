using Discord;
using Discord.Commands;
using DiscordUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SanaraV3.Module.Entertainment
{
    public sealed class JapaneseModule : ModuleBase
    {
        [Command("Manga", RunMode = RunMode.Async)]
        public async Task MangaAsync([Remainder] string name)
        {
            await SearchMediaAsync(JapaneseMedia.MANGA, name);
        }
        
        [Command("Anime", RunMode = RunMode.Async)]
        public async Task AnimeAsync([Remainder] string name)
        {
            await SearchMediaAsync(JapaneseMedia.ANIME, name);
        }

        [Command("Light novel", RunMode = RunMode.Async), Alias("LN")]
        public async Task LightNovelAsync([Remainder] string name)
        {
            await SearchMediaAsync(JapaneseMedia.LIGHT_NOVEL, name);
        }

        private async Task SearchMediaAsync(JapaneseMedia media, string query)
        {
            // Authentification is required to see NSFW content
            if (StaticObjects.KitsuAuth != null)
            {
                if (StaticObjects.KitsuAccessToken == null)
                {
                    var answer = await StaticObjects.HttpClient.SendAsync(StaticObjects.KitsuAuth);
                    var authJson = JsonConvert.DeserializeObject<JObject>(await answer.Content.ReadAsStringAsync());
                    StaticObjects.KitsuAccessToken = authJson["access_token"].Value<string>();
                    StaticObjects.KitsuRefreshDate = DateTime.Now.AddSeconds(authJson["expires_in"].Value<int>());
                    StaticObjects.KitsuRefreshToken = authJson["refresh_token"].Value<string>();
                }
                else if (DateTime.Now > StaticObjects.KitsuRefreshDate) // An access token last 30 days so we probably don't have to care about this but we do, just in case
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://kitsu.io/api/oauth/token")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "grant_type", "refresh_token" },
                            { "refresh_token", StaticObjects.KitsuRefreshToken }
                        })
                    };
                    var answer = await StaticObjects.HttpClient.SendAsync(request);
                    var authJson = JsonConvert.DeserializeObject<JObject>(await answer.Content.ReadAsStringAsync());
                    StaticObjects.KitsuAccessToken = authJson["access_token"].Value<string>();
                    StaticObjects.KitsuRefreshDate = DateTime.Now.AddSeconds(authJson["expires_in"].Value<int>());
                    StaticObjects.KitsuRefreshToken = authJson["refresh_token"].Value<string>();
                }
            }

            StaticObjects.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", StaticObjects.KitsuAccessToken); // TODO: Check if this can cause an issue with threads
            // For anime we need to contact the anime endpoint, manga and light novels are on the manga endpoint
            // The limit=5 is because we only take the 5 firsts results to not end up with things that are totally unrelated
            var json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("https://kitsu.io/api/edge/" + (media == JapaneseMedia.ANIME ? "anime" : "manga") + "?page[limit]=5&filter[text]=" + query));

            StaticObjects.HttpClient.DefaultRequestHeaders.Authorization = null;

            var data = json["data"].Value<JArray>();

            // Filter data depending of wanted media
            JToken[] allData;
            if (media == JapaneseMedia.MANGA)
                allData = data.Where(x => x["attributes"]["subtype"].Value<string>() != "novel").ToArray();
            else if (media == JapaneseMedia.LIGHT_NOVEL)
                allData = data.Where(x => x["attributes"]["subtype"].Value<string>() == "novel").ToArray();
            else
                allData = data.ToArray();

            if (allData.Length == 0)
                throw new CommandFailed("Nothing was found with this name.");

            string cleanName = Utils.CleanWord(query); // Cleaned word for comparaisons

            // If we can find an exact match, we go with that
            foreach (var elem in allData)
            {
                if (elem["attributes"]["canonicalTitle"].Value<string>() == query ||
                    elem["attributes"]["titles"]["en"] != null && elem["attributes"]["titles"]["en"].Value<string>() == query ||
                    elem["attributes"]["titles"]["en_jp"] != null && elem["attributes"]["titles"]["en_jp"].Value<string>() == query ||
                    elem["attributes"]["titles"]["en_us"] != null && elem["attributes"]["titles"]["en_us"].Value<string>() == query)
                {
                    await PostAnimeEmbedAsync(media, elem);
                    return;
                }
            }

            // Else we try to find something that somehow match
            foreach (var elem in allData)
            {
                if (Utils.CleanWord(elem["attributes"]["canonicalTitle"].Value<string>()) == query ||
                    elem["attributes"]["titles"]["en"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en"].Value<string>() ?? "").Contains(cleanName) ||
                    elem["attributes"]["titles"]["en_jp"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en_jp"].Value<string>() ?? "").Contains(cleanName) ||
                    elem["attributes"]["titles"]["en_us"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en_us"].Value<string>() ?? "").Contains(cleanName))
                {
                    // We would rather find the episodes and not some OVA/ONA
                    // We don't filter them before so we can fallback on them if we find nothing else
                    if (media == JapaneseMedia.ANIME && elem["attributes"]["subtype"].Value<string>() != "TV")
                        continue;

                    await PostAnimeEmbedAsync(media, elem);
                    return;
                }
            }

            // Otherwise, we just fall back on the first result available
            await PostAnimeEmbedAsync(media, allData[0]);
        }

        private async Task PostAnimeEmbedAsync(JapaneseMedia media, JToken token)
        {
            token = token["attributes"];

            if (Context.Channel is ITextChannel channel && !channel.IsNsfw && token["nsfw"] != null && token["nsfw"].Value<bool>())
                throw new CommandFailed("The result of your search was NSFW and thus, can only be shown in a NSFW channel.");

            var embed = new EmbedBuilder
            {
                Title = token["canonicalTitle"].Value<string>(),
                Color = token["nsfw"] == null ? Color.Green : (token["nsfw"].Value<bool>() ? new Color(255, 20, 147) : Color.Green),
                Url = "https://kitsu.io/" + (media == JapaneseMedia.ANIME ? "anime" : "manga") + "/" + token["slug"].Value<string>(),
                Description = token["synopsis"].Value<string>().Length > 1000 ? token["synopsis"].Value<string>().Substring(0, 1000) + " [...]" : token["synopsis"].Value<string>(), // Description that fill the whole screen are a pain
                ImageUrl = token["posterImage"]["original"].Value<string>()
            };
            if (token["titles"] != null && token["titles"]["en"] != null && !string.IsNullOrEmpty(token["titles"]["en"].Value<string>()) && token["titles"]["en"].Value<string>() != token["canonicalTitle"].Value<string>()) // No use displaying this if it's the same as the embed title
                embed.AddField("English Title", token["titles"]["en"].Value<string>());
            if (media == JapaneseMedia.ANIME && token["episodeCount"] != null)
                embed.AddField("Episode Count", token["episodeCount"].Value<string>() + (token["episodeLength"] != null ? $" ({token["episodeLength"].Value<string>()} minutes per episode)" : ""), true);
            if (token["startDate"] == null)
                embed.AddField("Release Date", "To Be Announced", true);
            else
                embed.AddField("Release Date", token["startDate"] + " - " + (token["endDate"] != null ? "???" : token["endDate"]), true);
            if (!string.IsNullOrEmpty(token["ageRatingGuide"].Value<string>()))
                embed.AddField("Audiance Warning", token["ageRatingGuide"].Value<string>(), true);
            if (!string.IsNullOrEmpty(token["averageRating"].Value<string>()))
                embed.AddField("Kitsu User Rating", token["averageRating"].Value<string>(), true);
            await ReplyAsync(embed: embed.Build());
        }
    }
}
