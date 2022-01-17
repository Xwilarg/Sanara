﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Exception;
using Sanara.Help;
using Sanara.Module.Utility;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

namespace Sanara.Module.Command.Impl
{
    public class Tool : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Tool", "Utility commands");
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "photo",
                        Description = "Find a photo given an optional query",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "query",
                                Description = "Filter the serach given a term",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
                            }
                        }
                    }.Build(),
                    callback: PhotoAsync,
                    precondition: Precondition.None,
                    needDefer: false
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "source",
                        Description = "Get the source of an image",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "image",
                                Description = "URL to the image",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        }
                    }.Build(),
                    callback: SourceAsync,
                    precondition: Precondition.None,
                    needDefer: false
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "anime",
                        Description = "Get information about an anime/manga/light novel",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "name",
                                Description = "Name",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            },
                            new SlashCommandOptionBuilder()
                            {
                                Name = "type",
                                Description = "What kind of media you are looking for",
                                Type = ApplicationCommandOptionType.Integer,
                                IsRequired = true,
                                Choices = new()
                                {
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Anime",
                                        Value = (int)JapaneseMedia.Anime
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Manga",
                                        Value = (int)JapaneseMedia.Manga
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Light Novel",
                                        Value = (int)JapaneseMedia.LightNovel
                                    }
                                }
                            }
                        }
                    }.Build(),
                    callback: AnimeAsync,
                    precondition: Precondition.None,
                    needDefer: false
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "drama",
                        Description = "Get information about a drama",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "name",
                                Description = "Name",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        }
                    }.Build(),
                    callback: DramaAsync,
                    precondition: Precondition.None,
                    needDefer: false
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "visualnovel",
                        Description = "Get information about a visual novel",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "name",
                                Description = "Name",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        }
                    }.Build(),
                    callback: VisualNovelAsync,
                    precondition: Precondition.None,
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "qrcode",
                        Description = "Generate a QR code",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "input",
                                Description = "Text hidden behind the QR Code",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        }
                    }.Build(),
                    callback: QrcodeAsync,
                    precondition: Precondition.None,
                    needDefer: false
                )
            };
        }

        public async Task QrcodeAsync(SocketSlashCommand ctx)
        {
            var input = (string)ctx.Data.Options.First(x => x.Name == "input").Value;
            await ctx.RespondWithFileAsync(
                fileStream: await StaticObjects.HttpClient.GetStreamAsync("https://api.qrserver.com/v1/create-qr-code/?data=" + HttpUtility.UrlEncode(input)),
                fileName: "qrcode.png",
                text: "",
                embeds: null,
                isTTS: false,
                ephemeral: false,
                allowedMentions: null,
                components: null,
                embed: null,
                options: null
            );
        }


        public async Task SourceAsync(SocketSlashCommand ctx)
        {
            var image = (string)ctx.Data.Options.First(x => x.Name == "image").Value;

            var html = await StaticObjects.HttpClient.GetStringAsync("https://saucenao.com/search.php?db=999&url=" + Uri.EscapeDataString(image));
            if (!html.Contains("<div id=\"middle\">"))
                throw new CommandFailed("I didn't find the source of this image.");
            var subHtml = html.Split(new[] { "<td class=\"resulttablecontent\">" }, StringSplitOptions.None)[1];

            var compatibility = float.Parse(Regex.Match(subHtml, "<div class=\"resultsimilarityinfo\">([0-9]{2,3}\\.[0-9]{1,2})%<\\/div>").Groups[1].Value, CultureInfo.InvariantCulture);
            var content = Utils.CleanHtml(subHtml.Split(new[] { "<div class=\"resultcontentcolumn\">" }, StringSplitOptions.None)[1].Split(new[] { "</div>" }, StringSplitOptions.None)[0]);
            var url = Regex.Match(html, "<img title=\"Index #[^\"]+\"( raw-rating=\"[^\"]+\") src=\"(https:\\/\\/img[0-9]+.saucenao.com\\/[^\"]+)\"").Groups[2].Value;

            await ctx.RespondAsync(embed: new EmbedBuilder
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

        public async Task PhotoAsync(SocketSlashCommand ctx)
        {
            if (StaticObjects.UnsplashToken == null)
            {
                throw new CommandFailed("Photo token is not available");
            }

            string? query = (string?)ctx.Data.Options.FirstOrDefault(x => x.Name == "query")?.Value;

            JObject json;
            if (query == null)
            {
                json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("https://api.unsplash.com/photos/random?client_id=" + StaticObjects.UnsplashToken));
            }
            else
            {
                var resp = await StaticObjects.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.unsplash.com/photos/random?query=" + HttpUtility.UrlEncode(query) + "&client_id=" + StaticObjects.UnsplashToken));
                if (resp.StatusCode == HttpStatusCode.NotFound)
                    throw new CommandFailed("There is no result with these search terms.");
                json = JsonConvert.DeserializeObject<JObject>(await resp.Content.ReadAsStringAsync());
            }
            await ctx.RespondAsync(embed: new EmbedBuilder
            {
                Title = "By " + json["user"]["name"].Value<string>(),
                Url = json["links"]["html"].Value<string>(),
                ImageUrl = json["urls"]["full"].Value<string>(),
                Footer = new EmbedFooterBuilder
                {
                    Text = json["description"].Value<string>()
                },
                Color = Color.Blue
            }.Build());
        }

        public async Task VisualNovelAsync(SocketSlashCommand ctx)
        {
            var name = (string)ctx.Data.Options.First(x => x.Name == "name").Value;
            string originalName = name;
            name = Utils.CleanWord(name);
            HttpWebRequest http = (HttpWebRequest)WebRequest.Create("https://vndb.org/v/all?sq=" + HttpUtility.UrlEncode(originalName).Replace("%20", "+"));
            http.AllowAutoRedirect = false;

            string html;
            HttpWebResponse response;
            // HttpClient doesn't really look likes to handle redirection properly
            try
            {
                response = (HttpWebResponse)http.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                response = (HttpWebResponse)ex.Response;
            }
            using Stream stream = response.GetResponseStream();
            using StreamReader reader = new(stream);
            html = reader.ReadToEnd();

            uint id = 0;
            if (response.StatusCode == HttpStatusCode.OK) // Search succeed
            {
                // Parse HTML and go though every VN, check the original name and translated name to get the VN id
                // TODO: Use string length for comparison
                MatchCollection matches = Regex.Matches(html, "<a href=\"\\/v([0-9]+)\" title=\"([^\"]+)\">([^<]+)<\\/a>");
                foreach (Match match in matches)
                {
                    string titleName = Utils.CleanWord(match.Groups[3].Value);
                    string titleNameBase = match.Groups[2].Value;
                    if (id == 0 && (titleName.Contains(name) || titleNameBase.Contains(originalName)))
                        id = uint.Parse(match.Groups[1].Value);
                    if (titleName == name || titleNameBase == name)
                    {
                        id = uint.Parse(match.Groups[1].Value);
                        break;
                    }
                }
                // If no matching name, we take the first one in the search list, if none these NotFound
                if (id == 0)
                {
                    if (matches.Count == 0)
                        throw new CommandFailed("Nothing was found with this name.");
                    id = uint.Parse(matches[0].Groups[1].Value);
                }
            }
            else // Only one VN found, search is trying to redirect us
            {
                id = uint.Parse(response.Headers["Location"][2..]); // VN ID is the location which is in format /VXXXX, XXXX being our numbers
            }


            var vn = (await StaticObjects.VnClient.GetVisualNovelAsync(VndbFilters.Id.Equals(id), VndbFlags.FullVisualNovel)).ToArray()[0];

            var embed = new EmbedBuilder()
            {
                Title = vn.OriginalName == null ? vn.Name : vn.OriginalName + " (" + vn.Name + ")",
                Url = "https://vndb.org/v" + vn.Id,
                ImageUrl = ctx.Channel is ITextChannel channel && !channel.IsNsfw && (vn.ImageRating.SexualAvg > 1 || vn.ImageRating.ViolenceAvg > 1) ? null : vn.Image,
                Description = vn.Description == null ? null : Regex.Replace(vn.Description.Length > 1000 ? vn.Description[0..1000] + " [...]" : vn.Description, "\\[url=([^\\]]+)\\]([^\\[]+)\\[\\/url\\]", "[$2]($1)"),
                Color = Color.Blue
            };
            embed.AddField("Available in english?", vn.Languages.Contains("en") ? "Yes" : "No", true);
            embed.AddField("Available on Windows?", vn.Platforms.Contains("win") ? "Yes" : "No", true);
            string length = "???";
            switch (vn.Length)
            {
                case VisualNovelLength.VeryShort: length = "<2 Hours"; break;
                case VisualNovelLength.Short: length = "2 - 10  Hours"; break;
                case VisualNovelLength.Medium: length = "10 - 30 Hours"; break;
                case VisualNovelLength.Long: length = "30 - 50 Hours"; break;
                case VisualNovelLength.VeryLong: length = "\\> 50 Hours"; break;
            }
            embed.AddField("Length", length, true);
            embed.AddField("Vndb Rating", vn.Rating + " / 10", true);
            string releaseDate;
            if (vn.Released?.Year == null)
                releaseDate = "TBA";
            else
            {
                releaseDate = vn.Released.Year.Value.ToString();
                if (!vn.Released.Month.HasValue)
                    releaseDate = $"{vn.Released.Month.Value:D2}/{releaseDate}";
                if (!vn.Released.Day.HasValue)
                    releaseDate = $"{vn.Released.Day.Value:D2}/{releaseDate}";
            }
            embed.AddField("Release Date", releaseDate, true);
            response.Dispose();
            await ctx.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
        }

        public async Task DramaAsync(SocketSlashCommand ctx)
        {
            if (StaticObjects.TranslationClient == null)
            {
                throw new CommandFailed("Drama token is not available");
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://api.mydramalist.com/v1/search/titles?q=" + HttpUtility.UrlEncode((string)ctx.Data.Options.ElementAt(0).Value)),
                Method = HttpMethod.Post
            };
            request.Headers.Add("mdl-api-key", StaticObjects.MyDramaListApiKey);

            var response = await StaticObjects.HttpClient.SendAsync(request);
            var searchResults = JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());

            if (searchResults.Count == 0)
                throw new CommandFailed("Nothing was found with this name.");

            var id = searchResults.First().Value<int>("id");
            var drama = await GetDramaAsync(id);

            var embed = new EmbedBuilder()
            {
                Title = drama.Value<string>("original_title"),
                Description = drama.Value<string>("synopsis"),
                Color = new Color(0, 97, 157),
                Url = drama.Value<string>("permalink"),
                ImageUrl = drama["images"].Value<string>("poster")
            };

            embed.AddField("English Title", drama.Value<string>("title"), true);
            embed.AddField("Country", drama.Value<string>("country"), true);
            embed.AddField("Episode Count", drama.Value<int>("episodes"), true);

            if (drama["released"] != null)
            {
                embed.AddField("Released", drama.Value<string>("released"), true);
            }
            else
            {
                embed.AddField("Air date", drama.Value<string>("aired_start") + ((drama["aired_end"] != null) ? " - " + drama.Value<string>("aired_end") : ""), true);
            }
            embed.AddField("Audiance Warning", drama.Value<string>("certification"), true);

            if (drama["votes"] != null)
            {
                embed.AddField("MyDramaList User Rating", drama.Value<double>("rating") + "/10", true);
            }

            await ctx.RespondAsync(embed: embed.Build());
        }

        public static async Task<JObject> GetDramaAsync(int id)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://api.mydramalist.com/v1/titles/" + id),
                Method = HttpMethod.Get
            };
            request.Headers.Add("mdl-api-key", StaticObjects.MyDramaListApiKey);

            var response = await StaticObjects.HttpClient.SendAsync(request);
            return JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
        }

        public async Task AnimeAsync(SocketSlashCommand ctx)
        {
            var name = (string)ctx.Data.Options.First(x => x.Name == "name").Value;
            var media = (JapaneseMedia)(long)ctx.Data.Options.First(x => x.Name == "type").Value;
            var answer = (await SearchMediaAsync(media, name)).Attributes;

            if (ctx.Channel is ITextChannel channel && !channel.IsNsfw && answer.Nsfw)
                throw new CommandFailed("The result of your search was NSFW and thus, can only be shown in a NSFW channel.");

            var description = "";
            if (!string.IsNullOrEmpty(answer.Synopsis))
                description = answer.Synopsis.Length > 1000 ? answer.Synopsis[..1000] + " [...]" : answer.Synopsis; // Description that fill the whole screen are a pain
            var embed = new EmbedBuilder
            {
                Title = answer.CanonicalTitle,
                Color = answer.Nsfw ? new Color(255, 20, 147) : Color.Green,
                Url = "https://kitsu.io/" + (media == JapaneseMedia.Anime ? "anime" : "manga") + "/" + answer.Slug,
                Description = description,
                ImageUrl = answer.PosterImage.Original
            };
            if (!string.IsNullOrEmpty(answer.Titles?.En) && answer.Titles?.En != answer.CanonicalTitle) // No use displaying this if it's the same as the embed title
                embed.AddField("English Title", answer.Titles!.En, true);
            if (!string.IsNullOrEmpty(null))
                embed.AddField("Kitsu User Rating", answer.AverageRating, true);
            if (media == JapaneseMedia.Anime && !string.IsNullOrEmpty(answer.EpisodeCount))
                embed.AddField("Episode Count", answer.EpisodeCount + (answer.EpisodeLength != null ? $" ({answer.EpisodeLength} min per episode)" : ""), true);
            if (!string.IsNullOrEmpty(answer.StartDate))
                embed.AddField("Release Date", "To Be Announced", true);
            else
                embed.AddField("Release Date", answer.StartDate + " - " + (answer.EndDate ?? "???"), true);
            if (!string.IsNullOrEmpty(answer.AgeRatingGuide))
                embed.AddField("Audiance Warning", answer.AgeRatingGuide, true);
            await ctx.RespondAsync(embed: embed.Build());
        }

        public static async Task<AnimeInfo> SearchMediaAsync(JapaneseMedia media, string query, bool onlyExactMatch = false)
        {
            // Authentification is required to see NSFW content
            if (StaticObjects.KitsuAuth != null)
            {
                if (StaticObjects.KitsuAccessToken == null) // Get access token
                {
                    var answer = await StaticObjects.HttpClient.SendAsync(StaticObjects.KitsuAuth);
                    var authJson = JsonConvert.DeserializeObject<JObject>(await answer.Content.ReadAsStringAsync());
                    StaticObjects.KitsuAccessToken = authJson["access_token"].Value<string>();
                    StaticObjects.KitsuRefreshDate = DateTime.Now.AddSeconds(authJson["expires_in"].Value<int>());
                    StaticObjects.KitsuRefreshToken = authJson["refresh_token"].Value<string>();
                }
                else if (DateTime.Now > StaticObjects.KitsuRefreshDate) // Access token expired, need to refresh it
                {
                    var tokenReq = new HttpRequestMessage(HttpMethod.Post, "https://kitsu.io/api/oauth/token")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "grant_type", "refresh_token" },
                            { "refresh_token", StaticObjects.KitsuRefreshToken }
                        })
                    };
                    var answer = await StaticObjects.HttpClient.SendAsync(tokenReq);
                    var authJson = JsonConvert.DeserializeObject<JObject>(await answer.Content.ReadAsStringAsync());
                    StaticObjects.KitsuAccessToken = authJson["access_token"].Value<string>();
                    StaticObjects.KitsuRefreshDate = DateTime.Now.AddSeconds(authJson["expires_in"].Value<int>());
                    StaticObjects.KitsuRefreshToken = authJson["refresh_token"].Value<string>();
                }
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://kitsu.io/api/edge/" + (media == JapaneseMedia.Anime ? "anime" : "manga") + "?page[limit]=5&filter[text]=" + HttpUtility.UrlEncode(query)),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", StaticObjects.KitsuAccessToken);
            // For anime we need to contact the anime endpoint, manga and light novels are on the manga endpoint
            // The limit=5 is because we only take the 5 firsts results to not end up with things that are totally unrelated
            var json = JsonConvert.DeserializeObject<JObject>(await (await StaticObjects.HttpClient.SendAsync(request)).Content.ReadAsStringAsync());

            var data = json!["data"]!.Value<JArray>()!.ToObject<AnimeInfo[]>();

            // Filter data depending of wanted media
            AnimeInfo[] allData;
            if (media == JapaneseMedia.Manga)
                allData = data.Where(x => x.Attributes.Subtype != "novel").ToArray();
            else if (media == JapaneseMedia.LightNovel)
                allData = data.Where(x => x.Attributes.Subtype == "novel").ToArray();
            else
                allData = data.ToArray();

            if (allData.Length == 0)
                throw new CommandFailed("Nothing was found with this name.");

            string cleanName = Utils.CleanWord(query); // Cleaned word for comparaisons

            // If we can find an exact match, we go with that
            string upperName = query.ToUpperInvariant();
            foreach (var elem in allData)
            {
                var answer = elem.Attributes;
                if (answer.CanonicalTitle.ToUpperInvariant() == upperName ||
                    answer.Titles.En?.ToUpperInvariant() == upperName ||
                    answer.Titles.En_jp?.ToUpperInvariant() == upperName ||
                    answer.Titles.En_us?.ToUpperInvariant() == upperName)
                {
                    return elem;
                }
            }

            if (onlyExactMatch) // Used by subscriptions (because we want to be 100% sure to not match the wrong anime)
                throw new CommandFailed("Nothing was found with this name");

            // Else we try to find something that somehow match
            foreach (var elem in allData)
            {
                var answer = elem.Attributes;
                if (Utils.CleanWord(answer.CanonicalTitle).Contains(cleanName) ||
                    Utils.CleanWord(answer.Titles.En?.ToUpperInvariant() ?? "").Contains(cleanName) ||
                    Utils.CleanWord(answer.Titles.En_jp?.ToUpperInvariant() ?? "").Contains(cleanName) ||
                    Utils.CleanWord(answer.Titles.En_us?.ToUpperInvariant() ?? "").Contains(cleanName))
                {
                    // We would rather find the episodes and not some OVA/ONA
                    // We don't filter them before so we can fallback on them if we find nothing else
                    if (media == JapaneseMedia.Anime && elem.Attributes.Subtype != "TV")
                        continue;

                    return elem;
                }
            }

            // Otherwise, we just fall back on the first result available
            return allData[0];
        }
    }
}
