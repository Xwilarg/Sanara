
/*

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadBooruHelp()
        {
            _submoduleHelp.Add("Booru", "Get anime images given some tags");

#if NSFW_BUILD
            var categoryName = "Nsfw";
#else
            var categoryName = "Images";
#endif


            _help.Add((categoryName, new Help("Booru", "Tags", new[] { new Argument(ArgumentType.MANDATORY, "id") }, "Get information about the tags of an image that was sent.", new string[0], Restriction.None, null)));
        }
    }
}

namespace SanaraV3.Module.Nsfw
{
    public sealed class BooruSfwModule : ModuleBase
    {
        [Command("Safebooru", RunMode = RunMode.Async)]
        public async Task SafebooruAsync(params string[] tags)
            => await BooruModule.SearchBooruAsync(StaticObjects.Safebooru, tags, BooruType.SAFEBOORU, Context.Channel);

        [Command("E926", RunMode = RunMode.Async)]
        public async Task E926Async(params string[] tags)
            => await BooruModule.SearchBooruAsync(StaticObjects.E926, tags, BooruType.E926, Context.Channel);

        [Command("Tags", RunMode = RunMode.Async)]
        public async Task TagsAsync(int id)
        {
            var info = await StaticObjects.Tags.GetTagAsync(id);
            if (!info.HasValue)
                throw new CommandFailed("There is no post with this id.");

            var res = info.Value;

            // If the post is SFW or if we are in a NSFW channel
            bool isPostAcceptable = res.Booru.IsSafe || (!res.Booru.IsSafe && Utils.CanSendNsfw(Context.Channel));
            int gcd = Utils.GCD(res.Post.Width, res.Post.Height);

            var embed = new EmbedBuilder
            {
                Color = BooruModule.RatingToColor(res.Post.Rating),
                Title = "From " + Utils.ToWordCase(res.Booru.ToString().Split('.').Last()),
                Description = res.Post.Width + " x " + res.Post.Height + "(" + (res.Post.Width / gcd) + ":" + (res.Post.Height / gcd) + ")",
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
                embed.Url = res.Post.PostUrl.AbsoluteUri;
            if (isPostAcceptable || res.Post.Rating == Rating.Safe)
                embed.ImageUrl = res.Post.PreviewUrl.AbsoluteUri;

            await ReplyAsync(embed: embed.Build());
        }
    }

    /// <summary>
    /// Contains all Booru related commands (booru are websites such as Gelbooru and Rule34)
    /// </summary>
    public sealed class BooruModule : ModuleBase
    {
        [Command("Gelbooru", RunMode = RunMode.Async), RequireNsfw]
        public async Task GelbooruAsync(params string[] tags)
            => await SearchBooruAsync(StaticObjects.Gelbooru, tags, BooruType.GELBOORU, Context.Channel);

        [Command("E621", RunMode = RunMode.Async), RequireNsfw]
        public async Task E621Async(params string[] tags)
            => await SearchBooruAsync(StaticObjects.E621, tags, BooruType.E621, Context.Channel);

        [Command("Rule34", RunMode = RunMode.Async), RequireNsfw]
        public async Task Rule34Async(params string[] tags)
            => await SearchBooruAsync(StaticObjects.Rule34, tags, BooruType.RULE34, Context.Channel);

        [Command("Konachan", RunMode = RunMode.Async), RequireNsfw]
        public async Task KonachanAsync(params string[] tags)
            => await SearchBooruAsync(StaticObjects.Konachan, tags, BooruType.KONACHAN, Context.Channel);

        [Command("Booru", RunMode = RunMode.Async), Alias("Image")]
        public async Task ImageAsync(params string[] tags)
        {
            if (Utils.CanSendNsfw(Context.Channel)) await SearchBooruAsync(StaticObjects.Gelbooru, tags, BooruType.GELBOORU, Context.Channel);
            else await SearchBooruAsync(StaticObjects.Safebooru, tags, BooruType.SAFEBOORU, Context.Channel);
        }

        public static async Task SearchBooruAsync(ABooru booru, string[] tags, BooruType booruId, IMessageChannel chan)
        {
            
        }
    }
}
*/