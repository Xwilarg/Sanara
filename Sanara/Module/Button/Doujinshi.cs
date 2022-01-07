using BooruSharp.Search.Post;
using Discord;
using Discord.WebSocket;
using Sanara.Exception;

namespace Sanara.Module.Button
{
    public class Doujinshi
    {
        public static async Task GetTagsAsync(SocketMessageComponent ctx, string id)
        {
            var info = await StaticObjects.Tags.GetTagAsync(id);
            if (!info.HasValue)
                throw new CommandFailed("There is no post with this id.");

            var res = info.Value;

            int gcd = Utils.GCD(res.Post.Width, res.Post.Height);

            var embed = new EmbedBuilder
            {
                Url = res.Post.PostUrl.AbsoluteUri,
                Color = res.Post.Rating switch
                {
                    Rating.Safe => Color.Green,
                    Rating.Questionable => new Color(255, 255, 0), // Yellow
                    Rating.Explicit => Color.Red,
                    _ => throw new NotImplementedException($"Invalid rating {res.Post.Rating}")
                },
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

            await ctx.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
        }
    }
}
