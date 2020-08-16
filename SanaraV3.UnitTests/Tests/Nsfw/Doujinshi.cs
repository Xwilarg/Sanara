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
        [Theory]
        [InlineData("kancolle")]
        [InlineData("ikazuchi color")]
        [InlineData("")]
        public async Task BooruTest(string tags)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                var embed = msg.Embeds.ElementAt(0);
                Assert.NotNull(embed.Image);
                Assert.NotNull(embed.Footer);
                Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
                Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
                isDone = true;
            });

            var mod = new Modules.Nsfw.DoujinshiModule();
            Common.AddContext(mod, callback);
            await mod.GetDoujinshi(tags);
            while (!isDone)
            { }
        }
    }
}
