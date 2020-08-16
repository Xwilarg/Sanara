using Discord;
using DiscordUtils;
using SanaraV3.UnitTests.Impl;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SanaraV3.UnitTests.Tests.Nsfw
{
    public sealed class Doujinshi
    {
        private async Task CheckEmbedAsync(Embed embed)
        {
            Assert.NotNull(embed.Image);
            Assert.NotNull(embed.Footer);
            Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
        }

        [Theory]
        [InlineData("kancolle")]
        [InlineData("ikazuchi color")]
        public async Task DoujinshiTest(string tags)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                await CheckEmbedAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Modules.Nsfw.DoujinshiModule();
            Common.AddContext(mod, callback);
            await mod.GetDoujinshiAsync(tags);
            while (!isDone)
            { }
        }


        [Fact]
        public async Task DoujinshiEmptyTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                await CheckEmbedAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Modules.Nsfw.DoujinshiModule();
            Common.AddContext(mod, callback);
            await mod.GetDoujinshiAsync();
            while (!isDone)
            { }
        }
    }
}
