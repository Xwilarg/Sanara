using NUnit.Framework;
using Sanara.Module.Utility;

namespace Sanara.UnitTests.Test
{
    public class NSFW : TestBase
    {
        [TestCase(BooruType.Safebooru, "")]
        [TestCase(BooruType.E621, "")]
        [TestCase(BooruType.Konachan, "")]
        [TestCase(BooruType.Safebooru, "moon")]
        [TestCase(BooruType.E621, "dragon")]
        [TestCase(BooruType.Konachan, "kimono")]
        public async Task BooruTest(BooruType source, string tags)
        {
            var mod = new Module.Command.Impl.Doujin();
            var ctx = new TestCommandContext(_provider, new()
            {
                { "source", (long)source },
                { "tags", tags }
            });
            await mod.BooruAsync(ctx);
            Assert.That($"From {source}", Is.EqualTo(ctx.Result.Embed.Title));
            //ClassicAssert.AreEqual(Color.Green, ctx.Result.Embed.Color);
            Assert.That(ctx.Result.Embed.Image.HasValue, Is.True);
            await AssertLinkAsync(ctx.Result.Embed.Image.Value.Url);
        }

        [TestCase("")]
        [TestCase("futanari")]
        public async Task BooruDanbooruTest(string tags)
        {
            var username = Environment.GetEnvironmentVariable("DANBOORU_USERNAME");
            var token = Environment.GetEnvironmentVariable("DANBOORU_APIKEY");

            if (username == null) Assert.Ignore();

            var mod = new Module.Command.Impl.Doujin();
            var ctx = new TestCommandContext(_provider, new()
            {
                { "source", (long)BooruType.Danbooru },
                { "tags", tags }
            });
            await mod.BooruAsync(ctx);
            Assert.That($"From {BooruType.Danbooru}", Is.EqualTo(ctx.Result.Embed.Title));
            //ClassicAssert.AreEqual(Color.Green, ctx.Result.Embed.Color);
            Assert.That(ctx.Result.Embed.Image.HasValue, Is.True);
            await AssertLinkAsync(ctx.Result.Embed.Image.Value.Url);
        }

        [Test]
        public async Task CosplayTest()
        {
            var mod = new Module.Command.Impl.Doujin();
            var ctx = new TestCommandContext(_provider, []);
            await mod.CosplayAsync(ctx);
            await AssertLinkAsync(ctx.Result.Embed.Image.Value.Url);
            await AssertLinkAsync(ctx.Result.Embed.Url);
        }
    }
}
