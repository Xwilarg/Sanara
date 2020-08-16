using Discord;
using DiscordUtils;
using SanaraV3.UnitTests.Impl;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SanaraV3.UnitTests.Tests.Nsfw
{
    public sealed class Cosplay
    {
        private async Task CheckEmbedAsync(Embed embed)
        {
            Assert.NotNull(embed.Image);
            Assert.NotNull(embed.Footer);
            Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
            Assert.Single(embed.Fields);
            Assert.InRange(double.Parse(embed.Fields[0].Value, CultureInfo.InvariantCulture), 0, 5);
        }

        [Theory]
        [InlineData("kancolle")]
        [InlineData("steinsgate mayuri")]
        public async Task CosplayTest(string tags)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                await CheckEmbedAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Modules.Nsfw.CosplayModule();
            Common.AddContext(mod, callback);
            await mod.Cosplay(tags);
            while (!isDone)
            { }
        }

        [Fact]
        public async Task CosplayEmptyTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                await CheckEmbedAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Modules.Nsfw.CosplayModule();
            Common.AddContext(mod, callback);
            await mod.Cosplay();
            while (!isDone)
            { }
        }
    }
}
