using Discord;
using Discord.Commands;
using DiscordUtils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace SanaraV3.UnitTests
{
    public class Tests
    {
        /// <summary>
        /// Create and assign a context to a module
        /// Since the SetContext method is private and from an interface we need to load the assemble and une reflection to get the method
        /// </summary>
        private void AddContext(ModuleBase module, Func<UnitTestUserMessage, Task> callback)
        {
            StaticObjects.Init();
            var assembly = Assembly.LoadFrom("Discord.Net.Commands.dll");
            var method = assembly.GetType("Discord.Commands.IModuleBase").GetMethod("SetContext", BindingFlags.Instance | BindingFlags.Public);
            var context = new CommandContext(new UnitTestDiscordClient(), new UnitTestUserMessage(callback));
            method.Invoke(module, new[] { context });
        }

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
                Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url+ " is not an image.");
                isDone = true;
            });

            var mod = new Modules.Entertainment.FunModule();
            AddContext(mod, callback);
            await mod.Inspire();
            while (!isDone)
            { }
        }

        /// <summary>
        /// Generic unit test method for all booru
        /// </summary>
        private async Task CheckBooruAsync(Embed embed)
        {
            Assert.NotNull(embed.Image);
            Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
            string title = embed.Title.Substring(5).ToLower();
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
            AddContext(mod, callback);
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
            AddContext(mod, callback);
            var method = typeof(Modules.Nsfw.BooruModule).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            await (Task)method.Invoke(mod, new[] { new[] { "kantai_collection" } });
            while (!isDone)
            { }
        }
    }
}
