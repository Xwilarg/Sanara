using Discord;
using DiscordUtils;
using NUnit.Framework;
using SanaraV3.UnitTests.Impl;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests.Tests.Nsfw
{
    [TestFixture]
    public sealed class Doujinshi
    {
        private async Task CheckEmbedAsync(Embed embed)
        {
            Assert.NotNull(embed.Image);
            Assert.NotNull(embed.Footer);
            Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
            Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url + " is not an image.");
        }

        [TestCase("hololive")]
        [TestCase("ikazuchi color")]
        public async Task DoujinshiTest(string tags)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckEmbedAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Module.Nsfw.DoujinModule();
            Common.AddContext(mod, callback);
            await mod.GetDoujinshiAsync(tags);
            while (!isDone)
            { }
        }


        [Test]
        public async Task DoujinshiEmptyTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckEmbedAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Module.Nsfw.DoujinModule();
            Common.AddContext(mod, callback);
            await mod.GetDoujinshiAsync();
            while (!isDone)
            { }
        }
    }
}
