using NUnit.Framework;

namespace Sanara.UnitTests.Test
{
    public class Entertainment : TestBase
    {

        [Test]
        public async Task VNQuoteTest()
        {
            var mod = new Module.Command.Impl.Media();
            var ctx = new TestCommandContext(_provider, []);
            await mod.VNQuoteAsync(ctx);
            await AssertLinkAsync(ctx.Result.Embed.Url);
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
