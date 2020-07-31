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
        private void AddContext(ModuleBase module, Action<UnitTestUserMessage> callback)
        {
            var assembly = Assembly.LoadFrom("Discord.Net.Commands.dll");
            var method = assembly.GetType("Discord.Commands.IModuleBase").GetMethod("SetContext", BindingFlags.Instance | BindingFlags.Public);
            var context = new CommandContext(new UnitTestDiscordClient(), new UnitTestUserMessage(callback));
            method.Invoke(module, new[] { context });
        }

        [Fact]
        public async Task InspireTest()
        {
            bool isDone = false;
            Action<UnitTestUserMessage> callback = new Action<UnitTestUserMessage>((msg) =>
            {
                Assert.Single(msg.Embeds);
                var embed = msg.Embeds.ElementAt(0);
                Assert.NotNull(embed.Image);
                Assert.True(Utils.IsLinkValid(embed.Image.Value.Url).GetAwaiter().GetResult(), embed.Image.Value.Url + " is not a valid URL.");
                Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url+ " is not an image.");
                isDone = true;
            });

            var mod = new Modules.Entertainment.FunModule();
            AddContext(mod, callback);
            await mod.Inspire();
            while (!isDone)
            { }
        }
    }
}
