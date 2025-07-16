using NUnit.Framework;

namespace Sanara.UnitTests.Test
{
    public class Entertainment : TestBase
    {
        [Test]
        public async Task VNQuoteTest()
        {
            var token = Environment.GetEnvironmentVariable("VNDB_TOKEN");

            if (token == null) Assert.Ignore();

            var mod = new Module.Command.Impl.Media();
            var ctx = new TestCommandContext(_provider, []);
            await mod.VNQuoteAsync(ctx);
            await AssertLinkAsync(ctx.Result.Embed.Url);
        }

        [Test]
        public async Task VnSearchTest()
        {
            var token = Environment.GetEnvironmentVariable("VNDB_TOKEN");

            if (token == null) Assert.Ignore();

            var mod = new Module.Command.Impl.Media();
            var ctx = new TestCommandContext(_provider, new Dictionary<string, object> { { "name", "maitetsu" } } );
            await mod.VisualNovelAsync(ctx);
            await AssertLinkAsync(ctx.Result.Embed.Url);
            Assert.Equals("Katawa Shoujo", ctx.Result.Embed.Title);
        }

        [Test]
        public async Task InspireTest()
        {
            var mod = new Module.Command.Impl.Media();
            var ctx = new TestCommandContext(_provider, []);
            await mod.InspireAsync(ctx);
            await AssertLinkAsync(ctx.Result.Embed.Image.Value.Url);
        }
    }
}
