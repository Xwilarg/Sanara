using SanaraV3.UnitTests.Impl;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SanaraV3.UnitTests.Tests.Tool
{
    public sealed class Language
    {
        [Theory]
        [InlineData("power plant", "power plant, power station", "発電所 - はつでんしょ (hatsudensho)")]
        [InlineData("妖精", "fairy, sprite, elf", "妖精 - ようせい (yousei)")]
        [InlineData("フランス", "France", "仏蘭西 - フランス (furansu)")]
        public async Task InspireTest(string entry, string title1, string contentLine1) // If the first line is okay, the rest should be okay too
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>((msg) =>
            {
                Assert.Single(msg.Embeds);
                var embed = msg.Embeds.ElementAt(0);
                Assert.Equal(5, embed.Fields.Length);
                var firstField = embed.Fields[0];
                Assert.Equal(title1, firstField.Name);
                Assert.Equal(contentLine1, firstField.Value.Split('\n')[0]);
                isDone = true;
                return Task.CompletedTask;
            });

            var mod = new Modules.Tool.LanguageModule();
            Common.AddContext(mod, callback);
            await mod.JapaneseAsync(entry);
            while (!isDone)
            { }
        }

        [Theory]
        [InlineData("つき", "tsuki")]
        [InlineData("にちじょう", "nichijou")]
        [InlineData("がっこう", "gakkou")]
        [InlineData("にゃんぱす", "nyanpasu")]
        [InlineData("フランス", "furansu")]
        [InlineData("aあアaあアaあア", "aaaaaaaaa")]
        [InlineData("エレベーター", "erebeta")]
        public void ToRomajiTest(string entry, string result)
        {
            Assert.Equal(result, Modules.Tool.LanguageModule.ToRomaji(entry));
        }
    }
}
