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
        [TestCase("夢", "夢", "dream, vision, illusion", "http://classic.jisho.org/static/images/stroke_diagrams/22818_frames.png", "夕: evening, sunset", new[] {
            "冖: wa-shaped crown radical (no. 14)",
            "夕: evening",
            "艾: moxa, sagebrush, wormwood, mugwort",
            "買: buy",
            "梦: dream, visionary, wishful",
            "夣: dream, to dream, visionary, stupid"
        }, new[] {
            "ム (mu)",
            "ボウ (bou)"
        }, new[] {
            "ゆめ (yume)",
            "ゆめ.みる (yume.miru)",
            "くら.い (kura.i)"
        })]
        [TestCase("tsuki", "月", "month, moon", "http://classic.jisho.org/static/images/stroke_diagrams/26376_frames.png", "月: moon, month", new[] {
            "月: month, moon"
        }, new[] {
            "ゲツ (getsu)",
            "ガツ (gatsu)"
        }, new[] {
            "つき (tsuki)"
        })]
        [TestCase("艦", "艦", "warship", "http://classic.jisho.org/static/images/stroke_diagrams/33382_frames.png", "舟: boat", new[] {
            "二: two, two radical (no. 7)",
            "皿: dish, a helping, plate",
            "舟: boat, ship",
            "臣: retainer, subject",
            "乞: beg, invite, ask"
        }, new[] {
            "カン (kan)"
        }, new[] {
            "None"
        })]
        public async Task KanjiTest(string entry, string title, string meaning, string imageUrl, string radical, string[] parts, string[] onyomi, string[] kunyomi)
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>((msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                var embed = msg.Embeds.ElementAt(0);
                Assert.AreEqual(4, embed.Fields.Length);
                Assert.AreEqual(title, embed.Title);
                Assert.AreEqual(meaning, embed.Description);
                Assert.True(embed.Image.HasValue);
                Assert.AreEqual(imageUrl, embed.Image.Value.Url);
                Assert.AreEqual(radical, embed.Fields[0].Value);

                var split1 = embed.Fields[1].Value.Split('\n');
                Assert.AreEqual(parts.Length, split1.Length);
                for (int i = 0; i < parts.Length; i++)
                    Assert.AreEqual(parts[i], split1[i]);

                var split2 = embed.Fields[2].Value.Split('\n');
                Assert.AreEqual(onyomi.Length, split2.Length);
                for (int i = 0; i < onyomi.Length; i++)
                    Assert.AreEqual(onyomi[i], split2[i]);

                var split3 = embed.Fields[3].Value.Split('\n');
                Assert.AreEqual(kunyomi.Length, split3.Length);
                for (int i = 0; i < kunyomi.Length; i++)
                    Assert.AreEqual(kunyomi[i], split3[i]);

                isDone = true;
                return Task.CompletedTask;
            });

            var mod = new Module.Tool.LanguageModule();
            Common.AddContext(mod, callback);
            await mod.KanjiAsync(entry);
            while (!isDone)
            { }
        }

        [TestCase("power plant", "power plant, power station", "発電所 - はつでんしょ (hatsudensho)")]
        [TestCase("妖精", "fairy, sprite, elf", "妖精 - ようせい (yousei)")]
        [TestCase("フランス", "France", "仏蘭西 - フランス (furansu)")]
        public async Task JapaneseTest(string entry, string title1, string contentLine1) // If the first line is okay, the rest should be okay too
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

            var mod = new Module.Tool.LanguageModule();
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
            Assert.AreEqual(result, Module.Tool.LanguageModule.ToRomaji(entry));
        }
    }
}
