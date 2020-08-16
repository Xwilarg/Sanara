using Discord;
using DiscordUtils;
using SanaraV3.UnitTests.Impl;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace SanaraV3.UnitTests.Tests.Nsfw
{
    public sealed class Booru
    {
        /// <summary>
        /// Generic unit test method for all booru
        /// </summary>
        private async Task CheckBooruAsync(Embed embed)
        {
            Assert.NotNull(embed.Image);
            Assert.NotNull(embed.Footer);
            Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
            string title = Regex.Match(embed.Title, "From ([a-zA-Z0-9]+)").Groups[1].Value.ToLower();
            Assert.Contains(title, embed.Url); // Title is for example For Gelbooru and url must be like "gelbooru.com/XXXX"
            Assert.Contains(title, embed.Image.Value.Url);
        }

        [Theory]
        [InlineData("E621")]
        [InlineData("E926")]
        [InlineData("Safebooru")]
        [InlineData("Gelbooru")]
        [InlineData("Rule34")]
        [InlineData("Konachan")]
        public async Task BooruTest(string methodName)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                await CheckBooruAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Modules.Nsfw.BooruModule();
            Common.AddContext(mod, callback);
            var method = typeof(Modules.Nsfw.BooruModule).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            await (Task)method.Invoke(mod, new object[] { null });
            while (!isDone)
            { }
        }

        [Theory]
        [InlineData("E621")]
        [InlineData("E926")]
        [InlineData("Safebooru")]
        [InlineData("Gelbooru")]
        [InlineData("Rule34")]
        [InlineData("Konachan")]
        public async Task BooruWithTagTest(string methodName)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                await CheckBooruAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Modules.Nsfw.BooruModule();
            Common.AddContext(mod, callback);
            var method = typeof(Modules.Nsfw.BooruModule).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            await (Task)method.Invoke(mod, new[] { new[] { "kantai_collection" } });
            while (!isDone)
            { }
        }

        [Theory]
        [InlineData("E621")]
        [InlineData("E926")]
        [InlineData("Safebooru")]
        [InlineData("Gelbooru")]
        [InlineData("Rule34")]
        [InlineData("Konachan")]
        public async Task BooruWithTagInvalidTest(string methodName)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                var embed = (Embed)msg.Embeds.ElementAt(0);
                await CheckBooruAsync(embed);
                Assert.Contains("arknights", embed.Footer.Value.Text);
                isDone = true;
            });

            var mod = new Modules.Nsfw.BooruModule();
            Common.AddContext(mod, callback);
            var method = typeof(Modules.Nsfw.BooruModule).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            await (Task)method.Invoke(mod, new[] { new[] { "arknigh" } });
            while (!isDone)
            { }
        }
    }
}
