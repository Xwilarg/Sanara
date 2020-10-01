using Discord;
using DiscordUtils;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using NUnit.Framework;
using SanaraV3.Exception;
using SanaraV3.UnitTests.Impl;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests.Tests.Entertainment
{
    [TestFixture]
    public sealed class Media
    {
        private async Task CheckRedditAsync(Embed embed, bool isImage)
        {
            if (isImage)
            {
                Assert.NotNull(embed.Image);
                Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url + " is not an image.");
            }
            Assert.NotNull(embed.Footer);
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
            Assert.NotNull(embed.Footer);
        }

        [Test]
        public void RedditRandomInvalid()
        {
            var callback = new Func<UnitTestUserMessage, Task>((msg) => Task.CompletedTask);

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            Assert.ThrowsAsync<CommandFailed>(async () => await mod.RedditRandomAsync("awawawawawawawawa"));
        }

        [Test]
        public void RedditTopInvalid()
        {
            var callback = new Func<UnitTestUserMessage, Task>((msg) => Task.CompletedTask);

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            Assert.ThrowsAsync<CommandFailed>(async () => await mod.RedditTopAsync("awawawawawawawawa"));
        }

        [Test]
        public void RedditRandomNotHandled()
        {
            var callback = new Func<UnitTestUserMessage, Task>((msg) => Task.CompletedTask);

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            Assert.ThrowsAsync<CommandFailed>(async () => await mod.RedditRandomAsync("KanMusuNights"));
        }

        [Test]
        public async Task RedditRandom()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckRedditAsync((Embed)msg.Embeds.ElementAt(0), true);
                isDone = true;
            });

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            await mod.RedditRandomAsync("arknuts");
            while (!isDone)
            { }
        }

        [Test]
        public async Task RedditHot()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckRedditAsync((Embed)msg.Embeds.ElementAt(0), false);
                isDone = true;
            });

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            await mod.RedditHotAsync("touhou");
            while (!isDone)
            { }
        }

        [Test]
        public async Task RedditNew()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckRedditAsync((Embed)msg.Embeds.ElementAt(0), false);
                isDone = true;
            });

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            await mod.RedditNewAsync("arknights");
            while (!isDone)
            { }
        }

        [Test]
        public async Task RedditTop()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckRedditAsync((Embed)msg.Embeds.ElementAt(0), false);
                isDone = true;
            });

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            await mod.RedditHotAsync("wholesomeyuri");
            while (!isDone)
            { }
        }

        private async Task CheckYoutubeAsync(Embed embed, string expectedTitle, string expectedDescription, int expectedViews, int expectedLikes, int expectedDislikes)
        {
            Assert.NotNull(embed.Image);
            Assert.NotNull(embed.Footer);
            Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
            Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url + " is not an image.");
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
            Assert.AreEqual(expectedTitle, embed.Title);
            //Assert.True(embed.Description.Contains(expectedDescription));
            var views = double.Parse(Regex.Match(embed.Footer.Value.Text, "Views: ([0-9k ]+)").Groups[1].Value.Replace(" ", "").Replace("k", "000"));
            var likes = double.Parse(Regex.Match(embed.Footer.Value.Text, "Likes: ([0-9k ]+)").Groups[1].Value.Replace(" ", "").Replace("k", "000"));
            var dislikes = double.Parse(Regex.Match(embed.Footer.Value.Text, "Dislikes: ([0-9k ]+)").Groups[1].Value.Replace(" ", "").Replace("k", "000"));
            Assert.True(views >= expectedViews, $"View count assert failed: {views} >= {expectedViews}");
            Assert.True(likes >= expectedLikes, $"Like count assert failed: {likes} >= {expectedLikes}");
            Assert.True(dislikes >= expectedDislikes, $"Dislike count assert failed: {dislikes} >= {expectedDislikes}");
        }

        private async Task CheckXkcdAsync(Embed embed)
        {
            Assert.NotNull(embed.Image);
            Assert.NotNull(embed.Footer);
            Assert.True(await Utils.IsLinkValid(embed.Image.Value.Url), embed.Image.Value.Url + " is not a valid URL.");
            Assert.True(Utils.IsImage(Path.GetExtension(embed.Image.Value.Url)), embed.Image.Value.Url + " is not an image.");
            Assert.True(await Utils.IsLinkValid(embed.Url), embed.Url + " is not a valid URL.");
        }

        [TestCase("eq8r1ZTma08", "命に嫌われている。／まふまふ【歌ってみた】", "命に嫌われている。 /カンザキイオリ", 73000000, 724000, 14000)]
        [TestCase("https://www.youtube.com/watch?v=R1u7oCJ8dK4", "【艦これ】ドレミゴール【皐月のオリジナル曲MV】＜キネマ106＞", "この曲だけで３種類目の動画になりました∩˘ω˘∩", 1000000, 10000, 70)]
        [TestCase("https://youtu.be/59jHEoNSlA0", "[Hatsune Miku] AaAaAaAAaAaAAa あぁあぁあぁああぁあぁああぁ PV (English Subtitles)", "Great video by NashimotoP", 700000, 29000, 200)]
        [TestCase("恋はどう？", "【BeatStream アニムトライヴ】『恋はどう？モロ◎波動OK☆方程式！！』", "リズムにあわせて画面をタッチする新快感音楽ゲーム「BeatStream　アニムトライヴ」収録ムービー。", 90000, 1000, 10)]
        public async Task YoutubeTest(string search, string expectedTitle, string expectedDescription, int expectedViews, int expectedLikes, int expectedDislikes)
        {
            var youtubeKey = Environment.GetEnvironmentVariable("YOUTUBE_KEY");
            if (youtubeKey == null)
                Assert.Ignore("Environment not available.");
            StaticObjects.YouTube = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = youtubeKey
            });

            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckYoutubeAsync((Embed)msg.Embeds.ElementAt(0), expectedTitle, expectedDescription, expectedViews, expectedLikes, expectedDislikes);
                isDone = true;
            });

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            await mod.YoutubeAsync(search);
            while (!isDone)
            { }
        }

        [Test]
        public async Task XkcdTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckXkcdAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            await mod.XkcdAsync();
            while (!isDone)
            { }
        }

        [Test]
        public async Task XkcdLastTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                await CheckXkcdAsync((Embed)msg.Embeds.ElementAt(0));
                isDone = true;
            });

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            await mod.XkcdLastAsync();
            while (!isDone)
            { }
        }

        [Test]
        public async Task XkcdWithIdTest()
        {
            bool isDone = false;
            var callback = new Func<UnitTestUserMessage, Task>(async (msg) =>
            {
                Assert.AreEqual(1, msg.Embeds.Count);
                var embed = (Embed)msg.Embeds.ElementAt(0);
                await CheckXkcdAsync(embed);
                Assert.AreEqual("https://imgs.xkcd.com/comics/workflow.png", embed.Image.Value.Url);
                Assert.AreEqual("https://xkcd.com/1172/", embed.Url);
                isDone = true;
            });

            var mod = new Module.Entertainment.MediaModule();
            Common.AddContext(mod, callback);
            await mod.XkcdAsync(1172);
            while (!isDone)
            { }
        }
    }
}
