using Discord;
using NUnit.Framework;

namespace Sanara.UnitTests.Test
{
    public class NSFW
    {
        [Test]
        public async Task BooruTest()
        {
            var mod = new Module.Command.Impl.NSFW();
            var ctx = new TestCommandContext(new()
            {
                { "source", 0L }
            });
            await mod.BooruAsync(ctx);
            Assert.AreEqual("From Safebooru", ctx.Result.Embed.Title);
            Assert.AreEqual(Color.Green, ctx.Result.Embed.Color);
            Assert.IsTrue(ctx.Result.Embed.Image.HasValue);
            Assert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Image.Value.Url));
        }

        [Test]
        public async Task DoujinshiTest()
        {
            var mod = new Module.Command.Impl.NSFW();
            var ctx = new TestCommandContext(new()
            {
                { "tags", "touhou" }
            });
            await mod.DoujinshiAsync(ctx);
            Assert.NotNull(ctx.Result.Embed.Title);
            Assert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Url));
            Assert.Contains("touhou project", ctx.Result.Embed.Description.Split(',').Select(x => x.Trim()).ToArray());
            Assert.IsTrue(ctx.Result.Embed.Image.HasValue);
            // Assert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Image.Value.Url));
        }

        [Test]
        public async Task CosplayTest()
        {
            var mod = new Module.Command.Impl.NSFW();
            var ctx = new TestCommandContext(new()
            {
                { "tags", "touhou" }
            });
            await mod.CosplayAsync(ctx);
            Assert.NotNull(ctx.Result.Embed.Title);
            Assert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Url));
            Assert.Contains("touhou project", ctx.Result.Embed.Description.Split(',').Select(x => x.Trim()).ToArray());
            Assert.IsTrue(ctx.Result.Embed.Image.HasValue);
            // Assert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Image.Value.Url));
        }
    }
}
