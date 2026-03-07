using NUnit.Framework;
using NUnit.Framework.Legacy;
using Sanara.Module.Utility;

namespace Sanara.UnitTests.Test
{
    public class NSFW : TestBase
    {
        [TestCase(BooruType.Safebooru)]
        //[TestCase(BooruType.E926)] // Somehow don't work in tests?
        [TestCase(BooruType.E621)]
        [TestCase(BooruType.Konachan)]
        [TestCase(BooruType.Sakugabooru)]
        public async Task BooruTest(BooruType source)
        {
            var mod = new Module.Command.Impl.Doujin();
            var ctx = new TestCommandContext(_provider, new()
            {
                { "source", (long)source }
            });
            await mod.BooruAsync(ctx);
            ClassicAssert.AreEqual($"From {source}", ctx.Result.Embed.Title);
            //ClassicAssert.AreEqual(Color.Green, ctx.Result.Embed.Color);
            ClassicAssert.IsTrue(ctx.Result.Embed.Image.HasValue);
            ClassicAssert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Image.Value.Url));
        }

        public async Task BooruDanbooruTest()
        {
            var username = Environment.GetEnvironmentVariable("DANBOORU_USERNAME");
            var token = Environment.GetEnvironmentVariable("DANBOORU_APIKEY");

            if (username == null) Assert.Ignore();

            var mod = new Module.Command.Impl.Doujin();
            var ctx = new TestCommandContext(_provider, new()
            {
                { "source", (long)BooruType.Danbooru }
            });
            await mod.BooruAsync(ctx);
            ClassicAssert.AreEqual($"From {BooruType.Danbooru}", ctx.Result.Embed.Title);
            //ClassicAssert.AreEqual(Color.Green, ctx.Result.Embed.Color);
            ClassicAssert.IsTrue(ctx.Result.Embed.Image.HasValue);
            ClassicAssert.IsTrue(await Utils.IsLinkValidAsync(ctx.Result.Embed.Image.Value.Url));
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
