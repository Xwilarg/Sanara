using DiscordUtils;
using NUnit.Framework;
using SanaraV3.UnitTests.Impl;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests.Tests.Entertainment
{
    [TestFixture]
    public sealed class Fun
    {
        [Test]
        public async Task InspireTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                var embed = msg.Embeds.ElementAt(0);
                Assert.NotNull(embed.Image);
                Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
                Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url + " is not an image.");
                isDone = true;
            });

            var mod = new Module.Entertainment.FunModule();
            Common.AddContext(mod, callback);
            await mod.InspireAsync();
            while (!isDone)
            { }
        }
    }
}
