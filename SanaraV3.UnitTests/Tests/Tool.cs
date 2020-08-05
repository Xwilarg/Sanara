using SanaraV3.UnitTests.Impl;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SanaraV3.UnitTests.Tests
{
    public sealed class Tool
    {
        [Theory]
        [InlineData("power plant", "power plant, power station", "発電所 - はつでんしょ")]
        [InlineData("妖精", "fairy, sprite, elf", "妖精 - ようせい")]
        [InlineData("フランス", "France", "仏蘭西 - フランス")]
        public async Task InspireTest(string entry, string title1, string contentLine1) // If the first line is okay, the rest should be okay too
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.Single(msg.Embeds);
                var embed = msg.Embeds.ElementAt(0);
                Assert.Equal(5, embed.Fields.Length);
                var firstField = embed.Fields[0];
                Assert.Equal(title1, firstField.Name);
                Assert.Equal(contentLine1, firstField.Value.Split('\n')[0]);
                isDone = true;
            });

            var mod = new Modules.Tool.LanguageModule();
            Common.AddContext(mod, callback);
            await mod.Japanese(entry);
            while (!isDone)
            { }
        }
    }
}
