using BooruSharp.Booru;
using BooruSharp.Search;
using BooruSharp.Search.Post;
using Discord;
using Discord.Commands;
using DiscordUtils;
using SanaraV3.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Module.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadBooruHelp()
        {
            _help.Add(new Help("Safebooru", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random image from Safebooru (only SFW images).", false));
            _help.Add(new Help("E926", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random image from E926 (only SFW furry images).", false));
            _help.Add(new Help("Gelbooru", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random image from Gelbooru (all kinds of anime images).", true));
            _help.Add(new Help("E621", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random image from E621 (only furry images).", true));
            _help.Add(new Help("Rule34", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random image from Rule34 (mostly weird images).", true));
            _help.Add(new Help("Konachan", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random image from Konachan (only images that can be used as wallpaper).", true));
            _help.Add(new Help("Booru", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random image from Safebooru or Gelbooru depending if you're on a NSFW or not.", false));
            _help.Add(new Help("Tags", new[] { new Argument(ArgumentType.MANDATORY, "id") }, "Get information about the tags of an image that was sent.", false));
        }
    }
}

namespace SanaraV3.Module.Nsfw
{
    /// <summary>
    /// Contains all Booru related commands (booru are websites such as Gelbooru and Rule34)
    /// </summary>
    public sealed class BooruModule : ModuleBase
    {
        [Command("Safebooru", RunMode = RunMode.Async)]
        public async Task SafebooruAsync(params string[] tags)
            => await SearchBooruAsync(StaticObjects.Safebooru, tags, BooruType.SAFEBOORU);

        [Command("E926", RunMode = RunMode.Async)]
        public async Task E926Async(params string[] tags)
            => await SearchBooruAsync(StaticObjects.E926, tags, BooruType.E926);

        [Command("Gelbooru", RunMode = RunMode.Async), RequireNsfw]
        public async Task GelbooruAsync(params string[] tags)
            => await SearchBooruAsync(StaticObjects.Gelbooru, tags, BooruType.GELBOORU);

        [Command("E621", RunMode = RunMode.Async), RequireNsfw]
        public async Task E621Async(params string[] tags)
            => await SearchBooruAsync(StaticObjects.E621, tags, BooruType.E621);

        [Command("Rule34", RunMode = RunMode.Async), RequireNsfw]
        public async Task Rule34Async(params string[] tags)
            => await SearchBooruAsync(StaticObjects.Rule34, tags, BooruType.RULE34);

        [Command("Konachan", RunMode = RunMode.Async), RequireNsfw]
        public async Task KonachanAsync(params string[] tags)
            => await SearchBooruAsync(StaticObjects.Konachan, tags, BooruType.KONACHAN);

        [Command("Booru", RunMode = RunMode.Async), Alias("Image")]
        public async Task ImageAsync(params string[] tags)
        {
            if (Utils.CanSendNsfw(Context.Channel)) await SearchBooruAsync(StaticObjects.Gelbooru, tags, BooruType.GELBOORU);
            else await SearchBooruAsync(StaticObjects.Safebooru, tags, BooruType.SAFEBOORU);
        }

        [Command("Tags", RunMode = RunMode.Async)]
        public async Task TagsAsync(int id)
        {
            var info = await StaticObjects.Tags.GetTagAsync(id);
            if (!info.HasValue)
                throw new CommandFailed("There is no post with this id.");

            var res = info.Value;

            // If the post is SFW or if we are in a NSFW channel
            bool isPostAcceptable = res.Booru.IsSafe() || (!res.Booru.IsSafe() && Utils.CanSendNsfw(Context.Channel));
            int gcd = Utils.GCD(res.Post.width, res.Post.height);

            var embed = new EmbedBuilder
            {
                Color = RatingToColor(res.Post.rating),
                Title = "From " + Utils.ToWordCase(res.Booru.ToString().Split('.').Last()),
                Description = res.Post.width + " x " + res.Post.height + "(" + (res.Post.width / gcd) + ":" + (res.Post.height / gcd) + ")",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Artists",
                        Value = res.Artists.Count == 0 ? "Unkown" : "`" + string.Join(", ", res.Artists) + "`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Characters",
                        Value = res.Characters.Count == 0 ? "Unkown" : "`" + string.Join(", ", res.Characters) + "`"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Sources",
                        Value = res.Sources.Count == 0 ? "Unkown" : "`" + string.Join(", ", res.Sources) + "`"
                    }
                }
            };

            if (isPostAcceptable)
                embed.Url = res.Post.postUrl.AbsoluteUri;
            if (isPostAcceptable || res.Post.rating == Rating.Safe)
                embed.ImageUrl = res.Post.previewUrl.AbsoluteUri;

            await ReplyAsync(embed: embed.Build());
        }

        private async Task SearchBooruAsync(ABooru booru, string[] tags, BooruType booruId)
        {
            // GetRandomImageAsync crash if we send it something null
            tags ??= new string[0];

            BooruSharp.Search.Post.SearchResult post;
            List<string> newTags = null;
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
                    var related = await new Konachan().GetTagsAsync(s); // Konachan have a feature where it can "autocomplete" a tag so we use it to guess what the user meant
                    if (related.Length == 0)
                        throw new CommandFailed("There is no image with those tags.");
                    newTags.Add(related.OrderBy(x => GetStringDistance(x.name, s)).First().name);
                }
                try
                {
                    // Once we got our new tags, we try doing a new search with them
                    post = await booru.GetRandomPostAsync(newTags.ToArray());
                }
                catch (InvalidTags)
                {
                    // Might happens if the Konachan tags don't exist in the current booru
                    throw new CommandFailed("There is no image with those tags.");
                }
            }

            int id = int.Parse("" + (int)booruId + post.id);
            StaticObjects.Tags.AddTag(id, booru, post);

            if (post.fileUrl == null)
                throw new CommandFailed("A post was found but no image was available.");
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = RatingToColor(post.rating),
                ImageUrl = post.fileUrl.AbsoluteUri,
                Url = post.postUrl.AbsoluteUri,
                Title = "From " + Utils.ToWordCase(booru.ToString().Split('.').Last()),
                Footer = new EmbedFooterBuilder
                {
                    Text = (newTags == null ? "" : "Some of your tags were invalid, the current search was done with: " + string.Join(", ", newTags) + "\n") +
                        "Do the 'Tags' command with then id '" + id + "' to have more information about this image."
                }
            }.Build());
        }

        private Color RatingToColor(Rating rating)
        {
            if (rating == Rating.Safe) return Color.Green;
            if (rating == Rating.Questionable) return new Color(255, 255, 0); // Yellow
            return Color.Red;
        }

        // From: https://gist.github.com/Davidblkx/e12ab0bb2aff7fd8072632b396538560
        private static int GetStringDistance(string a, string b)
        {
            var source1Length = a.Length;
            var source2Length = b.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; i++)
                matrix[i, 0] = i;
            for (var j = 0; j <= source2Length; j++)
                matrix[0, j] = j;

            // Calculate rows and columns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (b[j - 1] == a[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            return matrix[source1Length, source2Length];
        }
    }
}
