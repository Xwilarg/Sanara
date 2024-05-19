using Discord;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual("From Safebooru", ctx.Result.Embed.Title);
            ClassicAssert.AreEqual(Color.Green, ctx.Result.Embed.Color);
            ClassicAssert.IsTrue(ctx.Result.Embed.Image.HasValue);
            ClassicAssert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Image.Value.Url));
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
            ClassicAssert.NotNull(ctx.Result.Embed.Title);
            ClassicAssert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Url));
            ClassicAssert.Contains("touhou project", ctx.Result.Embed.Description.Split(',').Select(x => x.Trim()).ToArray());
            ClassicAssert.IsTrue(ctx.Result.Embed.Image.HasValue);
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
            ClassicAssert.NotNull(ctx.Result.Embed.Title);
            ClassicAssert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Url));
            ClassicAssert.Contains("touhou project", ctx.Result.Embed.Description.Split(',').Select(x => x.Trim()).ToArray());
            ClassicAssert.IsTrue(ctx.Result.Embed.Image.HasValue);
            // Assert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Image.Value.Url));
        }
    }
}
