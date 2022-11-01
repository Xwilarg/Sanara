using BooruSharp.Booru;
using BooruSharp.Search;
using BooruSharp.Search.Post;
using Discord;
using Newtonsoft.Json;
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
                    aliases: Array.Empty<string>(),
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
                            },
                            new SlashCommandOptionBuilder()
                            {
                                Name = "rating",
                                Description = "Minimum rating (between 2 and 5)",
                                Type = ApplicationCommandOptionType.Integer,
                                IsRequired = false
                            }
                        }
                    }.Build(),
                    callback: CosplayAsync,
                    precondition: Precondition.NsfwOnly,
                    aliases: Array.Empty<string>(),
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
                    aliases: Array.Empty<string>(),
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
                    aliases: new[] { "av" },
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
                                Description = "Tags of your search",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
                            },
                            new SlashCommandOptionBuilder()
                            {
                                Name = "rating",
                                Description = "Minimum rating (between 2 and 5)",
                                Type = ApplicationCommandOptionType.Integer,
                                IsRequired = false
                            }
                        }
                    }.Build(),
                    callback: DoujinshiAsync,
                    precondition: Precondition.NsfwOnly,
                    aliases: new[] { "doujin" },
                    needDefer: true
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "wholesome",
                        Description = "Get a random wholesome doujinshi"
                    }.Build(),
                    callback: WholesomeAsync,
                    precondition: Precondition.NsfwOnly,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                )
            };
        }

        public async Task WholesomeAsync(ICommandContext ctx)
        {
            var info = JsonConvert.DeserializeObject<WholesomeList>(await StaticObjects.HttpClient.GetStringAsync("https://wholesomelist.com/api/random")).entry;
            var token = $"doujinshi-{Guid.NewGuid()}/{Regex.Match(info.link, "([0-9]+)").Groups[1].Value}";
            StaticObjects.Doujinshis.Add(token);
            var button = new ComponentBuilder()
                .WithButton("Download", token);

            var embed = new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Title = info.title,
                Url = info.link,
                ImageUrl = info.image
            };
            if (info.tags != null)
            {
                embed.AddField("Tags", string.Join(", ", info.tags), true);
            }
            embed.AddField("Parody", info.parody, true);
            embed.AddField("Note", info.note, true);
            embed.WithFooter($"Tier: {info.tier}");

            await ctx.ReplyAsync(embed: embed.Build()/*, components: button.Build()*/);
        }

        public async Task AdultVideoAsync(ICommandContext ctx)
        {
            if (StaticObjects.JavmostCategories.Count == 0)
                throw new CommandFailed("Javmost categories aren't loaded yet, please retry later.");

            var tag = ctx.GetArgument<string>("tag") ?? "all";

            string url = "https://www.javmost.cx/category/" + tag;
            string html = await AdultVideo.DoJavmostHttpRequestAsync(url);
            int perPage = Regex.Matches(html, "<div class=\"card \">").Count; // Number of result per page
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
            var arr = html.Split(new[] { "<div class=\"card \">" }, StringSplitOptions.None).Skip(1).ToList(); // We remove things life header and stuff
            Match videoMatch = null;
            string[] videoTags = null;
            string previewUrl = "";
            int retryCount = 10;
            while (arr.Count > 0) // That shouldn't fail
            {
                string currHtml = arr[StaticObjects.Random.Next(arr.Count)];
                videoMatch = Regex.Match(currHtml, "<a href=\"(https:\\/\\/www\\.javmost\\.cx\\/([^\\/]+)\\/)\"");
                if (!videoMatch.Success)
                {
                    retryCount--;
                    if (retryCount == 0)
                    {
                        throw new InvalidOperationException("No video found after 10 tries...");
                    }
                    continue;
                }
                videoMatch = Regex.Match(currHtml, "<a href=\"(https:\\/\\/www\\.javmost\\.cx\\/([^\\/]+)\\/)\"");
                previewUrl = Regex.Match(currHtml, "data-src=\"([^\"]+)\"").Groups[1].Value;
                if (previewUrl.StartsWith("//"))
                    previewUrl = "https:" + previewUrl;
                videoTags = Regex.Matches(currHtml, "<a href=\"https:\\/\\/www\\.javmost\\.cx\\/category\\/([^\\/]+)\\/\"").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();
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

        public async Task DoujinshiAsync(ICommandContext ctx)
        {
            await GetEHentaiAsync(ctx, "doujinshi", 1021);
        }

        public async Task CosplayAsync(ICommandContext ctx)
        {
            await GetEHentaiAsync(ctx, "cosplay", 959);
        }

        private async Task GetEHentaiAsync(ICommandContext ctx, string name, int category)
        {
            var tags = ctx.GetArgument<string>("tags") ?? "";
            var ratingInput = ctx.GetArgument<long?>("rating") ?? 0;

            if (ratingInput != 0 && (ratingInput < 2 || ratingInput > 5))
            {
                throw new CommandFailed($"The rating given must be between 2 and 5");
            }

            var pageCount = await EHentai.GetEHentaiContentCountAsync(name, category, (int)ratingInput, tags);
            var rand = StaticObjects.Random.Next(0, pageCount);
            var matches = await EHentai.GetAllMatchesAsync(category, (int)ratingInput, tags, rand / 25); // There are 25 results by page
            var target = matches[rand % 25];
            await EHentai.SendEmbedAsync(ctx, name, target);
        }

        public async Task BooruAsync(ICommandContext ctx)
        {
            var tags = (ctx.GetArgument<string>("tags") ?? "").Split(' ');
            var type = (BooruType)ctx.GetArgument<long>("source");

            var booruMax = Enum.GetValues(typeof(BooruType)).Cast<int>().Max();
            if ((int)type < 0 || (int)type > booruMax)
            {
                throw new CommandFailed($"The booru given in parameter must be between 0 and {booruMax}");
            }

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
                throw new CommandFailed("This booru is only available in NSFW channels", true);
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

            string id = $"tags-{(int)type}{post.ID}";
            StaticObjects.Tags.AddTag(id, booru, post);

            if (post.FileUrl == null)
                throw new CommandFailed("A post was found but no image was available.");

            var button = new ComponentBuilder()
                .WithButton("Tags info", id);
            await ctx.ReplyAsync(embed: new EmbedBuilder
                {
                    Color = post.Rating switch
                    {
                        Rating.General => Color.Green,
                        Rating.Safe => Color.Green,
                        Rating.Questionable => new Color(255, 255, 0), // Yellow
                        Rating.Explicit => Color.Red,
                        _ => throw new NotImplementedException($"Invalid rating {post.Rating}")
                    },
                    ImageUrl = post.FileUrl.AbsoluteUri,
                    Url = post.PostUrl.AbsoluteUri,
                    Title = "From " + Utils.ToWordCase(booru.ToString().Split('.').Last())
                }.Build(), components: button.Build());

            await StaticObjects.Db.AddBooruAsync(type.ToString());
        }
    }
}
