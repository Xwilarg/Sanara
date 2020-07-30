using BooruSharp.Booru;
using BooruSharp.Search;
using BooruSharp.Search.Post;
using Discord;
using Discord.Commands;
using DiscordUtils;
using SanaraV3.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Nsfw
{
    public sealed class Booru : ModuleBase, IModule
    {
        public string GetModuleName()
            => "NSFW";

        [Command("Safebooru")]
        public async Task Safebooru(params string[] tags)
            => await SearchBooru(StaticObjects.Safebooru, tags);

        [Command("E926")]
        public async Task E926(params string[] tags)
            => await SearchBooru(StaticObjects.E926, tags);

        [Command("Gelbooru"), RequireNsfw]
        public async Task Gelbooru(params string[] tags)
            => await SearchBooru(StaticObjects.Gelbooru, tags);

        [Command("E621"), RequireNsfw]
        public async Task E621(params string[] tags)
            => await SearchBooru(StaticObjects.E621, tags);

        [Command("Rule34"), RequireNsfw]
        public async Task Rule34(params string[] tags)
            => await SearchBooru(StaticObjects.Rule34, tags);

        [Command("Konachan"), RequireNsfw]
        public async Task Konachan(params string[] tags)
            => await SearchBooru(StaticObjects.Konachan, tags);

        [Command("Booru"), Alias("Image")]
        public async Task Image(params string[] tags)
        {
            bool doesAllowNsfw = !(Context.Channel is ITextChannel) || ((ITextChannel)Context.Channel).IsNsfw;
            if (doesAllowNsfw) await SearchBooru(StaticObjects.Gelbooru, tags);
            else await SearchBooru(StaticObjects.Safebooru, tags);
        }

        private async Task SearchBooru(ABooru booru, string[] tags)
        {
            // GetRandomImageAsync crash if we send it something null
            if (tags == null)
                tags = new string[0];

            BooruSharp.Search.Post.SearchResult post;
            List<string> newTags = null;
            try
            {
                post = await booru.GetRandomImageAsync(tags);
            }
            catch (InvalidTags)
            {
                // On invalid tags we try to get guess which one the user wanted to use
                newTags = new List<string>();
                foreach (string s in tags)
                {
                    string tag = s;
                    var related = await new Konachan().GetTagsAsync(s); // Konachan have a feature where it can "autocomplete" a tag so we use it to guess what the user meant
                    if (related.Length == 0)
                        throw new CommandFailed("There is no image with those tags.");
                    newTags.Add(tag = related.OrderBy(x => GetStringDistance(x.name, s)).First().name);
                }
                try
                {
                    // Once we got our new tags, we try doing a new search with them
                    post = await booru.GetRandomImageAsync(newTags.ToArray());
                }
                catch (InvalidTags)
                {
                    // Might happens if the Konachan tags don't exist in the current booru
                    throw new CommandFailed("There is no image with those tags.");
                }
            }

            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = RatingToColor(post.rating),
                ImageUrl = post.fileUrl.AbsoluteUri,
                Url = post.postUrl.AbsoluteUri,
                Title = "From " + Utils.ToWordCase(booru.ToString().Split('.').Last()),
                Footer = new EmbedFooterBuilder
                {
                    Text = (newTags == null ? "" : "Some of your tags were invalid, the current search was done with: " + string.Join(", ", newTags) + "\n") +
                        "TODO: tags command"
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
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

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
