﻿using Discord;
using Discord.Commands;
using DiscordUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Attribute;
using SanaraV3.CustomClass;
using SanaraV3.Exception;
using SanaraV3.Subscription.Tags;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadJapaneseHelp()
        {
            _submoduleHelp.Add("Japanese", "Commands related to Japanese culture");
            _help.Add(("Entertainment", new Help("Japanese", "Manga", new[] { new Argument(ArgumentType.MANDATORY, "name") }, "Get information about a manga.", new string[0], Restriction.None, "Manga made in abyss")));
            _help.Add(("Entertainment", new Help("Japanese", "Anime", new[] { new Argument(ArgumentType.MANDATORY, "name") }, "Get information about an anime.", new string[0], Restriction.None, "Anime nichijou")));
            _help.Add(("Entertainment", new Help("Japanese", "Light Novel", new[] { new Argument(ArgumentType.MANDATORY, "name") }, "Get information about a light novel.", new string[]{ "LN" }, Restriction.None, "Light novel so I'm a spider so what")));
            _help.Add(("Entertainment", new Help("Japanese", "Subscribe anime", new[] { new Argument(ArgumentType.MANDATORY, "text channel") }, "Get information on all new anime in to a channel.", new string[0], Restriction.AdminOnly, null)));
            _help.Add(("Entertainment", new Help("Japanese", "Unsubscribe anime", new Argument[0], "Remove an anime subscription.", new string[0], Restriction.AdminOnly, null)));
            _help.Add(("Entertainment", new Help("Japanese", "Source", new[] { new Argument(ArgumentType.MANDATORY, "image") }, "Get the source of an image.", new string[0], Restriction.None, "Source https://sanara.zirk.eu/img/Gallery/001_01.jpg")));
        }
    }
}

namespace SanaraV3.Module.Entertainment
{
    public sealed class JapaneseModule : ModuleBase
    {
        [Command("VN"), Alias("Visual Novel")]
        public Task Community(params string[] _)
        {
            throw new NotYetAvailable();
        }

        [Command("Source", RunMode = RunMode.Async)]
        public async Task SourceAsync(ImageLink img)
        {
            await ParseSourceAsync(img.Link);
        }

        [Command("Source", RunMode = RunMode.Async)]
        public async Task SourceAsync()
        {
            if (Context.Message.Attachments.Count > 0 && Utils.IsImage(Path.GetExtension(Context.Message.Attachments.First().Url)))
                await ParseSourceAsync(Context.Message.Attachments.First().Url);
            else
                throw new CommandFailed("This command have some invalid parameters.");
        }

        private async Task ParseSourceAsync(string link)
        {
            var html = await StaticObjects.HttpClient.GetStringAsync("https://saucenao.com/search.php?db=999&url=" + Uri.EscapeDataString(link));
            if (!html.Contains("<div id=\"middle\">"))
                throw new CommandFailed("I didn't find the source of this image.");
            var subHtml = html.Split(new[] { "<td class=\"resulttablecontent\">" }, StringSplitOptions.None)[1];

            var compatibility = float.Parse(Regex.Match(subHtml, "<div class=\"resultsimilarityinfo\">([0-9]{2,3}\\.[0-9]{1,2})%<\\/div>").Groups[1].Value, CultureInfo.InvariantCulture);
            var content = Utils.CleanHtml(subHtml.Split(new[] { "<div class=\"resultcontentcolumn\">" }, StringSplitOptions.None)[1].Split(new[] { "</div>" }, StringSplitOptions.None)[0]);
            var url = Regex.Match(html, "<img title=\"Index #[^\"]+\"( raw-rating=\"[^\"]+\") src=\"(https:\\/\\/img[0-9]+.saucenao.com\\/[^\"]+)\"").Groups[2].Value;

            await ReplyAsync(embed: new EmbedBuilder
            {
                ImageUrl = url,
                Description = content,
                Color = Color.Green,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Certitude: {compatibility}%"
                }
            }.Build());
        }

        [Command("Subscribe anime"), RequireAdmin]
        public async Task SubscribeAnimeAsync(ITextChannel chan, params string[] tags)
        {
            await StaticObjects.Db.SetSubscriptionAsync(Context.Guild.Id, "anime", chan, new AnimeTags(tags, true));
            await ReplyAsync($"You subscribed for anime to {chan.Mention}.");
        }

        [Command("Unsubscribe anime"), RequireAdmin]
        public async Task UnsubscribeAnimeAsync()
        {
            if (!await StaticObjects.Db.HasSubscriptionExistAsync(Context.Guild.Id, "anime"))
                await ReplyAsync("There is no active anime subscription.");
            else
                await StaticObjects.Db.RemoveSubscriptionAsync(Context.Guild.Id, "anime");
        }

        [Command("Manga", RunMode = RunMode.Async)]
        public async Task MangaAsync([Remainder] string name)
        {
            await PostAnimeEmbedAsync(JapaneseMedia.MANGA, await SearchMediaAsync(JapaneseMedia.MANGA, name));
        }
        
        [Command("Anime", RunMode = RunMode.Async)]
        public async Task AnimeAsync([Remainder] string name)
        {
            await PostAnimeEmbedAsync(JapaneseMedia.ANIME, await SearchMediaAsync(JapaneseMedia.ANIME, name));
        }

        [Command("Light novel", RunMode = RunMode.Async), Alias("LN")]
        public async Task LightNovelAsync([Remainder] string name)
        {
            await PostAnimeEmbedAsync(JapaneseMedia.LIGHT_NOVEL, await SearchMediaAsync(JapaneseMedia.LIGHT_NOVEL, name));
        }

        public static async Task<JToken> SearchMediaAsync(JapaneseMedia media, string query, bool onlyExactMatch = false)
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
            string upperName = query.ToUpper();
            foreach (var elem in allData)
            {
                if (elem["attributes"]["canonicalTitle"].Value<string>().ToUpper() == upperName ||
                    elem["attributes"]["titles"]["en"] != null && elem["attributes"]["titles"]["en"].Value<string>().ToUpper() == upperName ||
                    elem["attributes"]["titles"]["en_jp"] != null && elem["attributes"]["titles"]["en_jp"].Value<string>().ToUpper() == upperName ||
                    elem["attributes"]["titles"]["en_us"] != null && elem["attributes"]["titles"]["en_us"].Value<string>().ToUpper() == upperName)
                {
                    return elem;
                }
            }

            if (onlyExactMatch) // Used by subscriptions (because we want to be 100% sure to not match the wrong anime)
                throw new CommandFailed("Nothing was found with this name");

            // Else we try to find something that somehow match
            foreach (var elem in allData)
            {
                if (Utils.CleanWord(elem["attributes"]["canonicalTitle"].Value<string>()).Contains(cleanName) ||
                    elem["attributes"]["titles"]["en"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en"].Value<string>() ?? "").Contains(cleanName) ||
                    elem["attributes"]["titles"]["en_jp"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en_jp"].Value<string>() ?? "").Contains(cleanName) ||
                    elem["attributes"]["titles"]["en_us"] != null && Utils.CleanWord(elem["attributes"]["titles"]["en_us"].Value<string>() ?? "").Contains(cleanName))
                {
                    // We would rather find the episodes and not some OVA/ONA
                    // We don't filter them before so we can fallback on them if we find nothing else
                    if (media == JapaneseMedia.ANIME && elem["attributes"]["subtype"].Value<string>() != "TV")
                        continue;

                    return elem;
                }
            }

            // Otherwise, we just fall back on the first result available
            return allData[0];
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
