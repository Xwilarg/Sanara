using Discord;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Sanara.Module.Command.Impl;

namespace Sanara.UnitTests.Test
{
    public class NSFW : TestBase
    {
        [Test]
        public async Task BooruTest()
        {
            var mod = new Module.Command.Impl.NSFW();
            var ctx = new TestCommandContext(_provider, new()
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
        public async Task CosplayTest()
        {
            var mod = new Module.Command.Impl.NSFW();
            var ctx = new TestCommandContext(_provider, []);
            await mod.CosplayAsync(ctx);
            await AssertLinkAsync(ctx.Result.Embed.Image.Value.Url);
            await AssertLinkAsync(ctx.Result.Embed.Url);
        }
    }
}
