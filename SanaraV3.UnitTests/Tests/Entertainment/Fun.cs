using DiscordUtils;
using SanaraV3.UnitTests.Impl;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SanaraV3.UnitTests.Tests.Entertainment
{
    public sealed class Fun
    {
        [Fact]
        public async Task InspireTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                var embed = msg.Embeds.ElementAt(0);
                Assert.NotNull(embed.Image);
                Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
                Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url + " is not an image.");
                isDone = true;
            });

            var mod = new Modules.Entertainment.FunModule();
            Common.AddContext(mod, callback);
            await mod.InspireAsync();
            while (!isDone)
            { }
        }

    }
}
