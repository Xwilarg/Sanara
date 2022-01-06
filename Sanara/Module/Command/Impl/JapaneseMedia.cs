using BooruSharp.Booru;
using BooruSharp.Search;
using BooruSharp.Search.Post;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Exception;
using Sanara.Help;
using Sanara.Module.Utility;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;


//            _help.Add(("Entertainment", new Help("Japanese", "Visual Novel", new[] { new Argument(ArgumentType.Mandatory, "name") }, "Get information about a visual novel.", new string[] { "VN" }, Restriction.None, "Visual novel katawa shoujo")));
//            _help.Add(("Entertainment", new Help("Japanese", "Subscribe anime", new[] { new Argument(ArgumentType.Mandatory, "text channel") }, "Get information on all new anime in to a channel.", Array.Empty<string>(), Restriction.AdminOnly, null)));
//            _help.Add(("Entertainment", new Help("Japanese", "Unsubscribe anime", Array.Empty<Argument>(), "Remove an anime subscription.", Array.Empty<string>(), Restriction.AdminOnly, null)));
//            _help.Add(("Entertainment", new Help("Japanese", "Source", new[] { new Argument(ArgumentType.Mandatory, "image") }, "Get the source of an image.", Array.Empty<string>(), Restriction.None, "Source https://sanara.zirk.eu/img/Gallery/001_01.jpg")));


namespace Sanara.Module.Command.Impl
{
    public sealed class JapaneseMedia : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Japanese Media", "Entertainement commands related to Japanese culture");
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
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
                                        Value = (int)Utility.JapaneseMedia.Anime
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Manga",
                                        Value = (int)Utility.JapaneseMedia.Manga
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Light Novel",
                                        Value = (int)Utility.JapaneseMedia.LightNovel
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
                    needDefer: false
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "booru",
                        Description = "Get an anime image",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "tags",
                                Description = "Tags of the search, separated by an empty space",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
                            },
                            new SlashCommandOptionBuilder()
                            {
                                Name = "source",
                                Description = "Where the image is coming from",
                                Type = ApplicationCommandOptionType.Integer,
                                IsRequired = true,
                                Choices = new()
                                {
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Safebooru (SFW)",
                                        Value = (int)BooruType.Safebooru
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "E926 (SFW, furry)",
                                        Value = (int)BooruType.E926
                                    },
#if NSFW_BUILD
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Gelbooru (NSFW)",
                                        Value = (int)BooruType.Gelbooru
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "E621 (NSFW, furry)",
                                        Value = (int)BooruType.E621
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Rule34 (NSFW, more variety of content)",
                                        Value = (int)BooruType.Rule34
                                    },
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Konachan (NSFW, wallpaper format)",
                                        Value = (int)BooruType.Konachan
                                    }
#endif
                                }
                            }
                        }
                    }.Build(),
                    callback: BooruAsync,
                    precondition: Precondition.None,
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "cosplay",
                        Description = "Get a cosplay",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "tags",
                                Description = "Tags of the search, separated by an empty space",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
                            }
                        }
                    }.Build(),
                    callback: CosplayAsync,
                    precondition: Precondition.NsfwOnly,
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "dlrand",
                        Description = "Get a random DLSite work"
                    }.Build(),
                    callback: DlRandAsync,
                    precondition: Precondition.NsfwOnly,
                    needDefer: true
                )
            };
        }

        public async Task DlRandAsync(SocketSlashCommand ctx)
        {
            var last = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
            var url = "https://www.dlsite.com/maniax/fsr/=/language/jp/regist_date_start/" + last.ToString("yyyy-MM-dd") + "/ana_flg/off/work_category%5B0%5D/doujin/order%5B0%5D/trend/per_page/30/release_term/week/show_type/1/from/fs.detail";

            var html = await StaticObjects.HttpClient.GetStringAsync(url);
            int maxId = int.Parse(Regex.Match(html, "RJ([0-9]+)\\.html").Groups[1].Value) + 1;
            int id;
            string doujinUrl;
            HttpResponseMessage msg;

            do
            {
                id = StaticObjects.Random.Next(1, maxId);
                doujinUrl = "https://www.dlsite.com/maniax/work/=/product_id/RJ" + id + ".html";
                msg = await StaticObjects.HttpClient.GetAsync(doujinUrl);
            } while (msg.StatusCode == HttpStatusCode.NotFound);
            html = await msg.Content.ReadAsStringAsync();

            var title = Regex.Match(html, "<meta property=\"og:title\" content=\"([^\"]+)").Groups[1].Value;
            title = HttpUtility.HtmlDecode(title[0..^9]);
            var imageUrl = Regex.Match(html, "<meta property=\"og:image\" content=\"([^\"]+)").Groups[1].Value;
            var description = HttpUtility.HtmlDecode(Regex.Match(html, "<meta name=\"description\" content=\"([^\"]+)").Groups[1].Value);
            var price = Regex.Match(html, "class=\"price[^\"]*\">([0-9,]+)").Groups[1].Value.Replace(',', ' ');
            var type = Regex.Match(html, "work_type[^\"]+\"><[^>]+>([^<]+)").Groups[1].Value;
            html = html.Contains("main_genre") ?
                html.Split(new[] { "main_genre" }, StringSplitOptions.None)[1].Split(new[] { "</div>" }, StringSplitOptions.None)[0]
                : "";
            var tags = Regex.Matches(html, "<a href=\"[^\"]+\">([^<]+)").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();

            await ctx.ModifyOriginalResponseAsync(x => x.Embed = new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Title = title,
                Url = doujinUrl,
                ImageUrl = imageUrl,
                Description = description,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Type",
                        Value = type,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Price",
                        Value = price + " ¥",
                        IsInline = true
                    }
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Tags: {string.Join(", ", tags)}"
                }
            }.Build());
        }

        public async Task CosplayAsync(SocketSlashCommand ctx)
        {
            var tags = (string)(ctx.Data.Options.FirstOrDefault(x => x.Name == "tags")?.Value ?? "");

            // 959 means we only take cosplays
            string url = "https://e-hentai.org/?f_cats=959&f_search=" + Uri.EscapeDataString(tags);
            string html = await StaticObjects.HttpClient.GetStringAsync(url);
            Match m = Regex.Match(html, "Showing ([0-9,]+) result"); // Get number of results

            if (!m.Success)
                throw new CommandFailed("There is no cosplay with these tags");

            int rand = StaticObjects.Random.Next(0, int.Parse(m.Groups[1].Value.Replace(",", ""))); // Number is displayed like 10,000 so we remove the comma to parse it
            html = await StaticObjects.HttpClient.GetStringAsync(url + "&page=" + (rand / 25)); // There are 25 results by page
            var sM = Regex.Matches(html, "<a href=\"(https:\\/\\/e-hentai\\.org\\/g\\/([a-z0-9]+)\\/([a-z0-9]+)\\/)\"")[rand % 25];
            string finalUrl = sM.Groups[1].Value;
            html = await StaticObjects.HttpClient.GetStringAsync(finalUrl);

            // Getting tags
            List<string> allTags = new();
            string htmlTags = html.Split(new[] { "taglist" }, StringSplitOptions.None)[1].Split(new[] { "Showing" }, StringSplitOptions.None)[0];
            foreach (Match match in Regex.Matches(htmlTags, ">([^<]+)<\\/a><\\/div>"))
                allTags.Add(match.Groups[1].Value);

            // To get the cover image, we first must go the first image of the gallery then we get it
            string htmlCover = await StaticObjects.HttpClient.GetStringAsync(Regex.Match(html, "<a href=\"([^\"]+)\"><img alt=\"0*1\"").Groups[1].Value);
            string imageUrl = Regex.Match(htmlCover, "<img id=\"img\" src=\"([^\"]+)\"").Groups[1].Value;

            // Getting rating
            string rating = Regex.Match(html, "average_rating = ([0-9.]+)").Groups[1].Value;

            var token = $"cosplay-{Guid.NewGuid()}/{sM.Groups[2].Value}/{sM.Groups[3].Value}";
            StaticObjects.Cosplays.Add(token);
            var button = new ComponentBuilder()
                .WithButton("Download", token);

            await ctx.ModifyOriginalResponseAsync(x =>
            {
                x.Embed = new EmbedBuilder
                {
                    Color = new Color(255, 20, 147),
                    Description = string.Join(", ", allTags),
                    Title = HttpUtility.HtmlDecode(Regex.Match(html, "<title>(.+) - E-Hentai Galleries<\\/title>").Groups[1].Value),
                    Url = finalUrl,
                    ImageUrl = imageUrl,
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Rating",
                            Value = rating,
                            IsInline = true
                        }
                    }
                }.Build();
                x.Components = button.Build();
            });
        }

        public async Task BooruAsync(SocketSlashCommand ctx)
        {
            var tags = ((string)(ctx.Data.Options.FirstOrDefault(x => x.Name == "tags")?.Value ?? "")).Split(' ');
            var type = (BooruType)(long)ctx.Data.Options.First(x => x.Name == "source").Value;

            ABooru booru = type switch
            {
                BooruType.Safebooru => StaticObjects.Safebooru,
                BooruType.Gelbooru => StaticObjects.Gelbooru,
                BooruType.E621 => StaticObjects.E621,
                BooruType.E926 => StaticObjects.E926,
                BooruType.Rule34 => StaticObjects.Rule34,
                BooruType.Konachan => StaticObjects.Konachan,
                _ => throw new NotImplementedException($"Invalid booru type {type}")
            };

            if (!booru.IsSafe && ctx.Channel is ITextChannel tChan && !tChan.IsNsfw)
            {
                throw new CommandFailed("This booru is only available in NSFW channels");
            }

            SearchResult post;
            List<string> newTags = new();
            try
            {
                post = await booru.GetRandomPostAsync(tags);
            }
            catch (InvalidTags)
            {
                // On invalid tags we try to get guess which one the user wanted to use
                newTags = new List<string>();
                foreach (string s in tags)
                {
                    var related = await StaticObjects.Konachan.GetTagsAsync(s); // Konachan have a feature where it can "autocomplete" a tag so we use it to guess what the user meant
                    if (related.Length == 0)
                        throw new CommandFailed("There is no image with those tags.");
                    newTags.Add(related.OrderBy(x => Utils.GetStringDistance(x.Name, s)).First().Name);
                }
                try
                {
                    // Once we got our new tags, we try doing a new search with them
                    post = await booru.GetRandomPostAsync(newTags.ToArray());
                }
                catch (InvalidTags)
                {
                    // Might happens if the Konachan tags don't exist in the current booru
                    throw new CommandFailed("There is no image with those tags");
                }
            }

            int id = int.Parse("" + (int)type + post.ID);
            StaticObjects.Tags.AddTag(id, booru, post);

            if (post.FileUrl == null)
                throw new CommandFailed("A post was found but no image was available.");

            await ctx.ModifyOriginalResponseAsync(x => x.Embed = new EmbedBuilder
            {
                Color = post.Rating switch
                {
                    Rating.Safe => Color.Green,
                    Rating.Questionable => new Color(255, 255, 0), // Yellow
                    Rating.Explicit => Color.Red,
                    _ => throw new NotImplementedException($"Invalid rating {post.Rating}")
                },
                ImageUrl = post.FileUrl.AbsoluteUri,
                Url = post.PostUrl.AbsoluteUri,
                Title = "From " + Utils.ToWordCase(booru.ToString().Split('.').Last()),
                Footer = new EmbedFooterBuilder
                {
                    Text = (newTags.Any() ? $"Some of your tags were invalid, the current search was done with: {string.Join(", ", newTags)}\n" : "") +
                        $"Do the 'Tags' command with then id '{id}' to have more information about this image."
                }
            }.Build());

            await StaticObjects.Db.AddBooruAsync(type.ToString());
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
            await ctx.RespondAsync(embed: embed.Build());
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
            var media = (Utility.JapaneseMedia)(long)ctx.Data.Options.First(x => x.Name == "type").Value;
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
                Url = "https://kitsu.io/" + (media == Utility.JapaneseMedia.Anime ? "anime" : "manga") + "/" + answer.Slug,
                Description = description,
                ImageUrl = answer.PosterImage.Original
            };
            if (!string.IsNullOrEmpty(answer.Titles?.En) && answer.Titles?.En != answer.CanonicalTitle) // No use displaying this if it's the same as the embed title
                embed.AddField("English Title", answer.Titles!.En, true);
            if (!string.IsNullOrEmpty(null))
                embed.AddField("Kitsu User Rating", answer.AverageRating, true);
            if (media == Utility.JapaneseMedia.Anime && !string.IsNullOrEmpty(answer.EpisodeCount))
                embed.AddField("Episode Count", answer.EpisodeCount + (answer.EpisodeLength != null ? $" ({answer.EpisodeLength} min per episode)" : ""), true);
            if (!string.IsNullOrEmpty(answer.StartDate))
                embed.AddField("Release Date", "To Be Announced", true);
            else
                embed.AddField("Release Date", answer.StartDate + " - " + (answer.EndDate ?? "???"), true);
            if (!string.IsNullOrEmpty(answer.AgeRatingGuide))
                embed.AddField("Audiance Warning", answer.AgeRatingGuide, true);
            await ctx.RespondAsync(embed: embed.Build());
        }

        public static async Task<AnimeInfo> SearchMediaAsync(Utility.JapaneseMedia media, string query, bool onlyExactMatch = false)
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
                RequestUri = new Uri("https://kitsu.io/api/edge/" + (media == Utility.JapaneseMedia.Anime ? "anime" : "manga") + "?page[limit]=5&filter[text]=" + HttpUtility.UrlEncode(query)),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", StaticObjects.KitsuAccessToken);
            // For anime we need to contact the anime endpoint, manga and light novels are on the manga endpoint
            // The limit=5 is because we only take the 5 firsts results to not end up with things that are totally unrelated
            var json = JsonConvert.DeserializeObject<JObject>(await (await StaticObjects.HttpClient.SendAsync(request)).Content.ReadAsStringAsync());

            var data = json!["data"]!.Value<JArray>()!.ToObject<AnimeInfo[]>();

            // Filter data depending of wanted media
            AnimeInfo[] allData;
            if (media == Utility.JapaneseMedia.Manga)
                allData = data.Where(x => x.Attributes.Subtype != "novel").ToArray();
            else if (media == Utility.JapaneseMedia.LightNovel)
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
                    if (media == Utility.JapaneseMedia.Anime && elem.Attributes.Subtype != "TV")
                        continue;

                    return elem;
                }
            }

            // Otherwise, we just fall back on the first result available
            return allData[0];
        }
        /*
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

*/
    }
}
