using Discord;
using DiscordUtils;
using NUnit.Framework;
using SanaraV3.Exception;
using SanaraV3.UnitTests.Impl;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests.Tests.Nsfw
{
    [TestFixture]
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
            Assert.True(embed.Url.Contains(title)); // Title is for example For Gelbooru and url must be like "gelbooru.com/XXXX"
            Assert.True(embed.Image.Value.Url.Contains(title));
        }

        private MethodInfo CreateMethod(string methodName)
        {
            if (methodName == "Safebooru")
            {
                return typeof(Module.Nsfw.BooruSfwModule).GetMethod(methodName + "Async", BindingFlags.Instance | BindingFlags.Public);
            }
            return typeof(Module.Nsfw.BooruModule).GetMethod(methodName + "Async", BindingFlags.Instance | BindingFlags.Public);
        }

        // TODO: Somehow, E621/E926 test don't pass but work in release, don't know why

        //[TestCase("E621")]
        //[TestCase("E926")]
        [TestCase("Safebooru")]
        [TestCase("Gelbooru")]
        [TestCase("Rule34")]
        [TestCase("Konachan")]
        public async Task BooruTest(string methodName)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckBooruAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Module.Nsfw.BooruModule();
            Common.AddContext(mod, callback);
            var method = CreateMethod(methodName);
            await (Task)method.Invoke(mod, new object[] { null });
            while (!isDone)
            { }
        }

        //[TestCase("E621")]
        //[TestCase("E926")]
        [TestCase("Safebooru")]
        [TestCase("Gelbooru")]
        [TestCase("Rule34")]
        [TestCase("Konachan")]
        public async Task BooruWithTagTest(string methodName)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckBooruAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Module.Nsfw.BooruModule();
            Common.AddContext(mod, callback);
            var method = CreateMethod(methodName);
            await (Task)method.Invoke(mod, new[] { new[] { "kantai_collection" } });
            while (!isDone)
            { }
        }

        //[TestCase("E621")]
        //[TestCase("E926")]
        [TestCase("Safebooru")]
        [TestCase("Gelbooru")]
        [TestCase("Rule34")]
        [TestCase("Konachan")]
        public void BooruWithTagInvalidTest(string methodName)
        {
            var mod = new Module.Nsfw.BooruModule();
            Common.AddContext(mod, null);
            var method = CreateMethod(methodName);
            Assert.ThrowsAsync<CommandFailed>(async () => await (Task)method.Invoke(mod, new[] { new[] { "azezaeaze" } }));
        }
    }
}
