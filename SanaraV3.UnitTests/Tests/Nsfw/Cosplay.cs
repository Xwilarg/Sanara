using Discord;
using DiscordUtils;
using NUnit.Framework;
using SanaraV3.UnitTests.Impl;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests.Tests.Nsfw
{
    [TestFixture]
    public sealed class Cosplay
    {
        private async Task CheckEmbedAsync(Embed embed)
        {
            Assert.NotNull(embed.Image);
            Assert.NotNull(embed.Footer);
            Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
            Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url + " is not an image.");
            Assert.AreEqual(1, embed.Fields.Length);
            var value = double.Parse(embed.Fields[0].Value, CultureInfo.InvariantCulture);
            Assert.True(value > 0);
            Assert.True(value < 5);
        }

        [TestCase("kancolle")]
        [TestCase("steinsgate mayuri")]
        public async Task CosplayTest(string tags)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckEmbedAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Modules.Nsfw.CosplayModule();
            Common.AddContext(mod, callback);
            await mod.CosplayAsync(tags);
            while (!isDone)
            { }
        }

        [Test]
        public async Task CosplayEmptyTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckEmbedAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Modules.Nsfw.CosplayModule();
            Common.AddContext(mod, callback);
            await mod.CosplayAsync();
            while (!isDone)
            { }
        }
    }
}
