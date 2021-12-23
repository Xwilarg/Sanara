using Discord;
using NUnit.Framework;
using Sanara.UnitTests.Impl;
using System.Globalization;

namespace Sanara.UnitTests.Tests.Nsfw
{
    [TestFixture]
    public sealed class Cosplay
    {/*
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

            var mod = new Module.Nsfw.CosplayModule();
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

            var mod = new Module.Nsfw.CosplayModule();
            Common.AddContext(mod, callback);
            await mod.CosplayAsync();
            while (!isDone)
            { }
        }*/
    }
}
