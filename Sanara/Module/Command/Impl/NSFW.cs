using BooruSharp.Booru;
using BooruSharp.Search;
using BooruSharp.Search.Post;
using Discord;
using Discord.WebSocket;
using NHentaiSharp.Core;
using NHentaiSharp.Exception;
using NHentaiSharp.Search;
using Sanara.Exception;
using Sanara.Help;
using Sanara.Module.Utility;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Sanara.Module.Command.Impl
{
    public sealed class NSFW : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("NSFW", "Commands to get lewd stuffs");
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
                
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "booru",
                        Description = "Get an anime image",
                        Options = new()
                        {
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
                            },
                            new SlashCommandOptionBuilder()
                            {
                                Name = "tags",
                                Description = "Tags of the search, separated by an empty space",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
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
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "adultvideo",
                        Description = "Get a random adult video work",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "tag",
                                Description = "Tag of the search",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false,
                                IsAutocomplete = true
                            }
                        }
                    }.Build(),
                    callback: AdultVideoAsync,
                    precondition: Precondition.NsfwOnly,
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "doujinshi",
                        Description = "Get a random doujinshi",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "tags",
                                Description = "Either the tags of your search, or a 6 digit number",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
                            }
                        }
                    }.Build(),
                    callback: DoujinshiAsync,
                    precondition: Precondition.NsfwOnly,
                    needDefer: true
                )
            };
        }

        public async Task DoujinshiAsync(ICommandContext ctx)
        {
            var tags = ctx.GetArgument<string>("tags");

            if (string.IsNullOrEmpty(tags))
            {
                var result = await SearchClient.SearchAsync();
                int page = StaticObjects.Random.Next(0, result.numPages) + 1;
                result = await SearchClient.SearchAsync(page);
                await FormatDoujinshiAsync(ctx, result.elements[StaticObjects.Random.Next(0, result.elements.Length)]);
            }
            else if (int.TryParse(tags, out int id))
            {
                try
                {
                    await FormatDoujinshiAsync(ctx, await SearchClient.SearchByIdAsync(id));
                }
                catch (InvalidArgumentException)
                {
                    throw new CommandFailed("These is no doujin with this id");
                }
            }
            else
            {
                // Somehow the API return invalid values depending of the country we are doing the request from
                // To avoid that, we parse the HTML instead
                string html = await StaticObjects.HttpClient.GetStringAsync("https://nhentai.net/search/?q=" + HttpUtility.UrlEncode(tags.Replace(" ", "+")));
                var m = Regex.Match(html, ";page=([0-9]+)\" class=\"last\"");
                if (!m.Success)
                {
                    throw new CommandFailed("There is no doujin with these tags");
                }
                int page = StaticObjects.Random.Next(0, int.Parse(m.Groups[1].Value)) + 1;
                html = await StaticObjects.HttpClient.GetStringAsync("https://nhentai.net/search/?q=" + HttpUtility.UrlEncode(tags.Replace(" ", "+")) + "&page=" + page);
                var matches = Regex.Matches(html, "<a href=\"\\/g\\/([0-9]+)\\/\"").Cast<Match>().ToArray();
                var result = await SearchClient.SearchByIdAsync(int.Parse(matches[StaticObjects.Random.Next(0, matches.Length)].Groups[1].Value));
                await FormatDoujinshiAsync(ctx, result);
            }
        }

        private async Task FormatDoujinshiAsync(ICommandContext ctx, GalleryElement result)
        {
            var token = $"doujinshi-{Guid.NewGuid()}/{result.id}";
            StaticObjects.Doujinshis.Add(token);
            var button = new ComponentBuilder()
                .WithButton("Download", token);

            var embed = new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Description = string.Join(", ", result.tags.Select(x => x.name)),
                Title = result.prettyTitle,
                Url = result.url.AbsoluteUri,
                ImageUrl = result.pages[0].imageUrl.AbsoluteUri
            }.Build();

            await ctx.ReplyAsync(embed: embed, components: button.Build());
        }

        public async Task AdultVideoAsync(ICommandContext ctx)
        {
            if (StaticObjects.JavmostCategories.Count == 0)
                throw new CommandFailed("Javmost categories aren't loaded yet, please retry later.");

            var tag = ctx.GetArgument<string>("tags") ?? "all";

            string url = "https://www5.javmost.com/category/" + tag;
            string html = await AdultVideo.DoJavmostHttpRequestAsync(url);
            int perPage = Regex.Matches(html, "<!-- begin card -->").Count; // Number of result per page
            int total = int.Parse(Regex.Match(html, "<input type=\"hidden\" id=\"page_total\" value=\"([0-9]+)\" \\/>").Groups[1].Value); // Total number of video
            if (total == 0)
            {
                throw new CommandFailed("There is nothing with this tag");
            }
            int page = StaticObjects.Random.Next(total / perPage);
            if (page > 0) // If it's the first page, we already got the HTML
            {
                html = await AdultVideo.DoJavmostHttpRequestAsync(url + "/page/" + (page + 1));
            }
            var arr = html.Split(new[] { "<!-- begin card -->" }, StringSplitOptions.None).Skip(1).ToList(); // We remove things life header and stuff
            Match videoMatch = null;
            string[] videoTags = null;
            string previewUrl = "";
            while (arr.Count > 0) // That shouldn't fail
            {
                string currHtml = arr[StaticObjects.Random.Next(arr.Count)];
                videoMatch = Regex.Match(currHtml, "<a href=\"(https:\\/\\/www\\.javmost\\.xyz\\/([^\\/]+)\\/)\"");
                if (!videoMatch.Success)
                    continue;
                videoMatch = Regex.Match(currHtml, "<a href=\"(https:\\/\\/www\\.javmost\\.xyz\\/([^\\/]+)\\/)\"");
                previewUrl = Regex.Match(currHtml, "data-src=\"([^\"]+)\"").Groups[1].Value;
                if (previewUrl.StartsWith("//"))
                    previewUrl = "https:" + previewUrl;
                videoTags = Regex.Matches(currHtml, "<a href=\"https:\\/\\/www\\.javmost\\.xyz\\/category\\/([^\\/]+)\\/\"").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();
                break;
            }
            await ctx.ReplyAsync(embed: new EmbedBuilder()
            {
                Color = new Color(255, 20, 147),
                Description = string.Join(", ", videoTags),
                Title = videoMatch.Groups[2].Value,
                Url = videoMatch.Groups[1].Value,
                ImageUrl = previewUrl
            }.Build());
        }

        public async Task DlRandAsync(ICommandContext ctx)
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

            await ctx.ReplyAsync(embed: new EmbedBuilder
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

        public async Task CosplayAsync(ICommandContext ctx)
        {
            var tags = ctx.GetArgument<string>("tags") ?? "";

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

            await ctx.ReplyAsync(embed: new EmbedBuilder
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
            }.Build(), components: button.Build());
        }

        public async Task BooruAsync(ICommandContext ctx)
        {
            var tags = (ctx.GetArgument<string>("tags") ?? "").Split(' ');
            var type = (BooruType)ctx.GetArgument<long>("source");

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

            BooruSharp.Search.Post.SearchResult post;
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

            string id = $"{(int)type}{post.ID}";
            StaticObjects.Tags.AddTag(id, booru, post);

            if (post.FileUrl == null)
                throw new CommandFailed("A post was found but no image was available.");

            var token = $"cosplay-{Guid.NewGuid()}/{id}";
            StaticObjects.Doujinshis.Add(token);
            var button = new ComponentBuilder()
                .WithButton("Tags info", token);
            await ctx.ReplyAsync(embed: new EmbedBuilder
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
                }.Build(), components: button.Build());

            await StaticObjects.Db.AddBooruAsync(type.ToString());
        }
    }
}
