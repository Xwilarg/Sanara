using BooruSharp.Booru;
using BooruSharp.Search;
using BooruSharp.Search.Post;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Exception;
using Sanara.Help;
using Sanara.Module.Utility;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sanara.Module.Command.Impl
{
    public sealed class NSFW : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("NSFW", "Commands to get lewd stuffs");
        }

        public CommandData[] GetCommands()
        {
            return [
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                    .WithName("cosplay")
                    .WithDescription("Get a cosplay")
                    .WithNsfw(true)
                    .AddOptions(GetEHentaiOptions()),
                callback: CosplayAsync,
                aliases: []
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                    .WithName("doujinshi")
                    .WithDescription("Get a fan-made manga")
                    .WithNsfw(true)
                    .AddOptions(GetEHentaiOptions()),
                callback: DoujinshiAsync,
                aliases: [ "doujin" ]
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                    .WithName("wholesome")
                    .WithDescription("Get a random wholesome NSFW fan-made manga")
                    .WithNsfw(true),
                callback: WholesomeAsync,
                aliases: []
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                    .WithName("adultvideo")
                    .WithDescription("Get a random Japanese Adult Video")
                    .AddOption(
                        new SlashCommandOptionBuilder()
                            .WithName("query")
                            .WithDescription("Search query")
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(false)
                    )
                    .WithNsfw(true),
                callback: AdultVideoAsync,
                aliases: [ "av", "jav" ]
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                    .WithName("booru")
                    .WithDescription("Get an anime image")
                    .WithNsfw(false)
                    .AddOptions(
                        new SlashCommandOptionBuilder()
                            .WithName("source")
                            .WithDescription("Where the image is coming from")
                            .WithType(ApplicationCommandOptionType.Integer)
                            .WithRequired(false)
                            .AddChoice("Safebooru", (int)BooruType.Safebooru)
                            .AddChoice("E926", (int)BooruType.E926)
                            .AddChoice("Sakugabooru (anime clips)", (int)BooruType.Sakugabooru)
#if NSFW_BUILD
                            .AddChoice("Gelbooru (NSFW)", (int)BooruType.Gelbooru)
                            .AddChoice("E621 (NSFW, furry)", (int)BooruType.E621)
                            .AddChoice("Rule34 (NSFW, more variety of content)", (int)BooruType.Rule34)
                            .AddChoice("Konachan (NSFW, wallpaper format)", (int)BooruType.Konachan)
#endif
                            ,
                        new SlashCommandOptionBuilder()
                            .WithName("tags")
                            .WithDescription("Tags of the search, separated by an empty space")
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(false)
                    ),
                callback: BooruAsync,
                aliases: []
            )
        ];
        }


        private SlashCommandOptionBuilder[] GetEHentaiOptions()
            => [
                    new SlashCommandOptionBuilder()
                    .WithName("tags")
                    .WithDescription("List of tags matching your search")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(false),
                new SlashCommandOptionBuilder()
                    .WithName("rating")
                    .WithDescription("Minimum rating of the search (default: 3)")
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithMinValue(1)
                    .WithMaxValue(5)
                    .WithRequired(false),
                new SlashCommandOptionBuilder()
                    .WithName("nsfw")
                    .WithDescription("Choose if the option should be NSFW or not")
                    .WithType(ApplicationCommandOptionType.Boolean)
                    .WithRequired(false)
                ];

        public async Task WholesomeAsync(IContext ctx)
        {
            var info = JsonSerializer.Deserialize<WholesomeList>(await ctx.Provider.GetRequiredService<HttpClient>().GetStringAsync("https://wholesomelist.com/api/random"), ctx.Provider.GetRequiredService<JsonSerializerOptions>()).Entry;

            var embed = new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Title = info.Title,
                Url = $"https://wholesomelist.com/list/{info.Uuid}",
                ImageUrl = info.Image
            };
            var components = new ComponentBuilder();

            if (info.Tags != null)
            {
                embed.AddField("Tags", string.Join(", ", info.Tags));
            }
            if (info.Note != null)
            {
                embed.AddField("Note", info.Note, true);
            }
            embed.AddField("Pages", $"{info.Pages} pages", true);
            if (info.Parody != null)
            {
                embed.AddField("Parody", info.Parody, true);
            }
            if (info.EH != null)
            {
                components.WithButton("Download", EHentai.GetEHentaiButton(info.EH));
            }
            embed.WithFooter($"Tier: {info.Tier}");

            await ctx.ReplyAsync(embed: embed.Build(), components: components.Build());
        }

        public async Task AdultVideoAsync(IContext ctx)
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

        public async Task CosplayAsync(IContext ctx)
        {
            var tags = ctx.GetArgument<string>("tags") ?? "";
            var nsfwFilter = ctx.GetArgument<bool?>("nsfw");
            if (nsfwFilter.HasValue)
            {
                if (nsfwFilter.Value) tags = $"-other:non-nude {tags}";
                else tags = $"other:non-nude {tags}";
            }

            await EHentai.GetEHentaiAsync(ctx, tags, "cosplay", 959);
        }

        public async Task DoujinshiAsync(IContext ctx)
        {
            var tags = ctx.GetArgument<string>("tags") ?? "";
            var nsfwFilter = ctx.GetArgument<bool?>("nsfw");

            int searchTarget = nsfwFilter switch
            {
                null => 253,
                true => 509,
                false => 767
            };

            await EHentai.GetEHentaiAsync(ctx, tags, "cosplay", searchTarget);
        }

        public async Task BooruAsync(IContext ctx)
        {
            var tags = (ctx.GetArgument<string>("tags") ?? string.Empty).Split(' ');
            var type = (BooruType)(ctx.GetArgument<long?>("source") ??
#if NSFW_BUILD
                (ctx.Channel is ITextChannel tChan && !tChan.IsNsfw ? (int)BooruType.Safebooru : (int)BooruType.Gelbooru)

#else
            (int)BooruType.Safebooru
#endif

            );

            ABooru booru = type switch
            {
                BooruType.Safebooru => new Safebooru(),
                BooruType.E926 => new E926(),
                BooruType.Sakugabooru => new Sakugabooru(),
#if NSFW_BUILD
                BooruType.Gelbooru => new Gelbooru(),
                BooruType.E621 => new E621(),
                BooruType.Rule34 => new Rule34(),
                BooruType.Konachan => new Konachan(),
#endif
                _ => throw new NotImplementedException($"Invalid booru type {type}")
            };

            var isChanSfw = ctx.Channel is ITextChannel textC && !textC.IsNsfw;
            if (isChanSfw && !booru.IsSafe && type != BooruType.Sakugabooru)
            {
                throw new CommandFailed("NSFW booru can only be requested in NSFW channels", ephemeral: true);
            }

            SearchResult post;
            List<string> newTags = [];
            try
            {
                post = await booru.GetRandomPostAsync(tags);
            }
            catch (InvalidTags)
            {
                // On invalid tags we try to get guess which one the user wanted to use
                newTags = [];
                foreach (string s in tags)
                {
                    var related = await new Konachan().GetTagsAsync(s); // Konachan have a feature where it can "autocomplete" a tag so we use it to guess what the user meant
                    if (related.Length == 0)
                        throw new CommandFailed("There is no image with those tags.");
                    newTags.Add(related.OrderBy(x => Utils.GetStringDistance(x.Name, s)).First().Name);
                }
                try
                {
                    // Once we got our new tags, we try doing a new search with them
                    post = await booru.GetRandomPostAsync([.. newTags]);
                }
                catch (InvalidTags)
                {
                    // Might happens if the Konachan tags don't exist in the current booru
                    throw new CommandFailed("There is no image with those tags");
                }
            }

            if (!isChanSfw && post.Rating == Rating.Explicit)
            {
                throw new CommandFailed("The image found have an unexpected rating of explicit");
            }

            var embed = new EmbedBuilder
            {
                Color = post.Rating switch
                {
                    Rating.General => Color.Green,
                    Rating.Safe => Color.Green,
                    Rating.Questionable => new Color(255, 255, 0),
                    Rating.Explicit => Color.Red,
                    _ => throw new NotImplementedException($"Invalid rating {post.Rating}")
                },
                Url = post.PostUrl.AbsoluteUri,
                Title = "From " + Utils.ToWordCase(booru.ToString().Split('.').Last())
            };

            if (post.DetailedTags != null)
            {
                var copyrights = post.DetailedTags.Where(x => x.Type == BooruSharp.Search.Tag.TagType.Copyright);
                var characters = post.DetailedTags.Where(x => x.Type == BooruSharp.Search.Tag.TagType.Character);
                var artists = post.DetailedTags.Where(x => x.Type == BooruSharp.Search.Tag.TagType.Artist);
                if (copyrights.Any())
                {
                    embed.AddField("Copyrights", string.Join(", ", copyrights.Select(x => x.Name)));
                }
                if (characters.Any())
                {
                    embed.AddField("Characters", string.Join(", ", characters.Select(x => x.Name)));
                }
                if (artists.Any())
                {
                    embed.AddField("Artists", string.Join(", ", artists.Select(x => x.Name)));
                }
            }

            var ext = Path.GetExtension(post.FileUrl.AbsoluteUri);
            if (post.FileUrl == null)
            {
                embed.Description = "This post doesn't have any image associated";
                await ctx.ReplyAsync(embed: embed.Build());
            }
            else if (Utils.IsImage(ext))
            {
                embed.ImageUrl = post.FileUrl.AbsoluteUri;
                await ctx.ReplyAsync(embed: embed.Build());
            }
            else if (ext == ".swf")
            {
                embed.Description = "Flash games cannot be previewed";
                await ctx.ReplyAsync(embed: embed.Build());
            }
            else
            {
                using MemoryStream ms = new(await ctx.Provider.GetRequiredService<HttpClient>().GetByteArrayAsync(post.FileUrl.AbsoluteUri));
                await ctx.ReplyAsync(ms, $"image{ext}", embed: embed.Build());
            }
        }
    }
}
