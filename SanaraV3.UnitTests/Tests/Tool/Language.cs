using NUnit.Framework;
using SanaraV3.UnitTests.Impl;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests.Tests.Tool
{
    [TestFixture]
    public sealed class Language
    {
        [TestCase("power plant", "power plant, power station", "発電所 - はつでんしょ (hatsudensho)")]
        [TestCase("妖精", "fairy, sprite, elf", "妖精 - ようせい (yousei)")]
        [TestCase("フランス", "France", "仏蘭西 - フランス (furansu)")]
        public async Task InspireTest(string entry, string title1, string contentLine1) // If the first line is okay, the rest should be okay too
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>((msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                var embed = msg.Embeds.ElementAt(0);
                Assert.AreEqual(5, embed.Fields.Length);
                var firstField = embed.Fields[0];
                Assert.AreEqual(title1, firstField.Name);
                Assert.AreEqual(contentLine1, firstField.Value.Split('\n')[0]);
                isDone = true;
                return Task.CompletedTask;
            });

            var mod = new Modules.Tool.LanguageModule();
            Common.AddContext(mod, callback);
            await mod.JapaneseAsync(entry);
            while (!isDone)
            { }
        }

        [TestCase("つき", "tsuki")]
        [TestCase("にちじょう", "nichijou")]
        [TestCase("がっこう", "gakkou")]
        [TestCase("にゃんぱす", "nyanpasu")]
        [TestCase("フランス", "furansu")]
        [TestCase("aあアaあアaあア", "aaaaaaaaa")]
        [TestCase("エレベーター", "erebeta")]
        public void ToRomajiTest(string entry, string result)
        {
            Assert.AreEqual(result, Modules.Tool.LanguageModule.ToRomaji(entry));
        }
    }
}
