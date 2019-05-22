using Xunit;
using System;
using SanaraV2.Features.NSFW;
using System.Net;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.IO;
using Discord;
using System.Linq;
using SanaraV2.Games;

namespace Sanara_UnitTests
{
    public class Program : IClassFixture<Tests>
    {
        private static bool IsLinkValid(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                try
                {
                    WebRequest request = WebRequest.Create(url);
                    request.Method = "HEAD";
                    request.GetResponse();
                    return (true);
                }
                catch (WebException)
                { }
            }
            return (false);
        }

        // ANIME/MANGA MODULE
        [Fact]
        public async Task TestAnime()
        {
            var result = await SanaraV2.Features.Entertainment.AnimeManga.SearchAnime(true, ("Gochuumon wa Usagi desu ka?").Split(' '));
            Assert.Equal(SanaraV2.Features.Entertainment.Error.AnimeManga.None, result.error);
            Assert.NotNull(result.answer);
            Assert.Equal("Gochuumon wa Usagi desu ka?", result.answer.name);
            Assert.Equal("https://media.kitsu.io/anime/poster_images/8095/original.jpg?1408463456", result.answer.imageUrl);
            Assert.Equal("GochiUsa", string.Join("", result.answer.alternativeTitles));
            Assert.Equal(12, result.answer.episodeCount);
            Assert.Equal(23, result.answer.episodeLength);
            Assert.InRange(result.answer.rating.Value, 60, 90);
            Assert.Equal(new DateTime(2014, 4, 10), result.answer.startDate);
            Assert.Equal(new DateTime(2014, 6, 26), result.answer.endDate);
            Assert.Equal("Teens 13 or older", result.answer.ageRating);
            Assert.InRange(result.answer.synopsis.Length, 800, 1200);
        }

        // DOUJINSHI MODULE
        [Fact]
        public async Task TestDoujinshi()
        {
           var result = await Doujinshi.SearchDoujinshi(false, new string[] { "color", "english" }, new Random());
            Assert.Equal(SanaraV2.Features.NSFW.Error.Doujinshi.None, result.error);
            Assert.True(IsLinkValid(result.answer.url));
        }

        // BOORU MODULE
        [Fact]
        public async Task TestBooruSafe()
        {
            var result = await Booru.SearchBooru(false, null, new BooruSharp.Booru.Safebooru(), new Random());
            Assert.Equal(Error.Booru.None, result.error);
            Assert.Equal(Color.Green, result.answer.colorRating);
            Assert.True(IsLinkValid(result.answer.url));
        }

        // GAMES INFO
        [Fact]
        public async Task TestKancolleCharac()
        {
            var result = await SanaraV2.Features.GamesInfo.Kancolle.SearchCharac(new[] { "Ryuujou" });
            Assert.Equal(SanaraV2.Features.GamesInfo.Error.Charac.None, result.error);
            Assert.Equal("https://vignette.wikia.nocookie.net/kancolle/images/5/59/Ryuujou_Card.png", result.answer.thumbnailUrl);
            Assert.Equal("Ryuujou", result.answer.name);
            Assert.Contains(result.answer.allCategories, x => x.Item1 == "Appearance");
            Assert.Contains(result.answer.allCategories, x => x.Item1 == "Personality");
            Assert.Contains(result.answer.allCategories, x => x.Item1 == "Trivia");
        }

        [Fact]
        public async Task TestKancolleMap()
        {
            var result = await SanaraV2.Features.GamesInfo.Kancolle.SearchDropMap(new[] { "Shimakaze" });
            Assert.Equal(SanaraV2.Features.GamesInfo.Error.Drop.None, result.error);
            Assert.InRange(result.answer.dropMap.Count, 5, 30); // Shimakaze drop will probably ever be between 5 and 20 maps
        }

        [Fact]
        public async Task TestKancolleConstruction()
        {
            var result = await SanaraV2.Features.GamesInfo.Kancolle.SearchDropConstruction(new[] { "Taihou" });
            Assert.Equal(SanaraV2.Features.GamesInfo.Error.Drop.None, result.error);
            Assert.NotEmpty(result.answer.elems);
            Assert.InRange(int.Parse(result.answer.elems[0].chance.Split('.')[0]), 2, 10);
            Assert.InRange(int.Parse(result.answer.elems[0].ammos), 1000, 10000);
            Assert.InRange(int.Parse(result.answer.elems[0].fuel), 1000, 10000);
            Assert.InRange(int.Parse(result.answer.elems[0].bauxite), 1000, 10000);
            Assert.InRange(int.Parse(result.answer.elems[0].iron), 1000, 10000);
            Assert.InRange(int.Parse(result.answer.elems[0].devMat), 1, 100);
        }

        // GAMES MODULE
        [Fact]
        public async Task TestGames()
        {
            foreach (var game in Constants.allDictionnaries)
            {
                Assert.NotNull(game.Item2);
                Assert.True(game.Item2.Count > 100);
            }
        }

        [Fact]
        public async Task TestBooruNotSafe()
        {
            var result = await Booru.SearchBooru(false, new string[] { "cum_in_pussy" }, new BooruSharp.Booru.Gelbooru(), new Random());
            Assert.Equal(Error.Booru.None, result.error);
            Assert.Equal(Color.Red, result.answer.colorRating);
            Assert.True(IsLinkValid(result.answer.url));
        }

        [Fact]
        public async Task TestBooruTag()
        {
            BooruSharp.Booru.Gelbooru booru = new BooruSharp.Booru.Gelbooru();
            Random rand = new Random();
            var resultSearch = await Booru.SearchBooru(false, new string[] { "hibiki_(kantai_collection)" }, booru, rand);
            var resultTags = await Booru.SearchTags(new string[] { resultSearch.answer.saveId.ToString() });
            Assert.Equal(Error.BooruTags.None, resultTags.error);
            Assert.Contains("hibiki_(kantai_collection)", resultTags.answer.characTags);
            Assert.Contains("kantai_collection", resultTags.answer.sourceTags);
            Assert.Equal("Gelbooru", resultTags.answer.booruName);
        }

        [SkipIfNoToken]
        public async Task IntegrationTest()
        {
            await SkipIfNoToken.chan.SendMessageAsync(SkipIfNoToken.ayamiMention + " safebooru");
            bool msgReceived = false;
            SkipIfNoToken.SubscribeMsg((arg) =>
            {
                if (arg.Author.Id != SkipIfNoToken.ayamiId)
                    return Task.CompletedTask;
                Assert.Equal(1, arg.Embeds.Count);
                EmbedImage? img = arg.Embeds.ToArray()[0].Image;
                Assert.True(img.HasValue);
                Assert.True(IsLinkValid(img.Value.Url));
                msgReceived = true;
                return Task.CompletedTask;
            });
            while (!msgReceived) { }
        }
    }

    public class Tests : IDisposable
    {
        public void Dispose()
        {
            foreach (var msg in SkipIfNoToken.chan.GetMessagesAsync().FlattenAsync().GetAwaiter().GetResult())
                msg.DeleteAsync().GetAwaiter().GetResult();
        }
    }

    public sealed class SkipIfNoToken : FactAttribute
    {
        private static bool inamiLoad = false, ayamiLoad = false;
        private static DiscordSocketClient inamiClient;
        public static ITextChannel chan;
        public static string ayamiMention;
        public static ulong ayamiId;

        public SkipIfNoToken()
        {
            Timeout = 30000;

            string ayamiToken, inamiToken, guildTesting, channelTesting;
            if (File.Exists("Keys/ayamiToken.txt"))
                ayamiToken = File.ReadAllText("Keys/ayamiToken.txt");
            else
                ayamiToken = Environment.GetEnvironmentVariable("AYAMI_TOKEN");
            if (File.Exists("Keys/inamiToken.txt"))
                inamiToken = File.ReadAllText("Keys/inamiToken.txt");
            else
                inamiToken = Environment.GetEnvironmentVariable("INAMI_TOKEN");
            if (File.Exists("Keys/guildTesting.txt"))
                guildTesting = File.ReadAllText("Keys/guildTesting.txt");
            else
                guildTesting = Environment.GetEnvironmentVariable("GUILD_TESTING");
            if (File.Exists("Keys/channelTesting.txt"))
                channelTesting = File.ReadAllText("Keys/channelTesting.txt");
            else
                channelTesting = Environment.GetEnvironmentVariable("CHANNEL_TESTING");
            if (ayamiToken == null || inamiToken == null || guildTesting == null || channelTesting == null)
            {
                Skip = "No token found in files or environment variables";
                return;
            }
            inamiClient = new DiscordSocketClient();
            inamiClient.LoginAsync(TokenType.Bot, inamiToken).GetAwaiter().GetResult();
            inamiClient.Connected += ConnectedInami;
            inamiClient.StartAsync().GetAwaiter().GetResult();
            while (!inamiLoad) // Waiting for Inami to connect...
            { }
            SanaraV2.Program ayamiProgram = new SanaraV2.Program();
            ayamiProgram.client.Connected += ConnectedAyami;
            ayamiProgram.MainAsync(ayamiToken, inamiClient.CurrentUser.Id).GetAwaiter().GetResult();
            while (!ayamiLoad) // Waiting for Ayami to connect...
            { }
            chan = inamiClient.GetGuild(ulong.Parse(guildTesting)).GetTextChannel(ulong.Parse(channelTesting));
            ayamiId = ayamiProgram.client.CurrentUser.Id;
            ayamiMention = "<@" + ayamiId + ">";
        }

        private Task ConnectedAyami()
        {
            ayamiLoad = true;
            return Task.CompletedTask;
        }

        private Task ConnectedInami()
        {
            inamiLoad = true;
            return Task.CompletedTask;
        }

        public static void SubscribeMsg(Func<SocketMessage, Task> fct)
        {
            inamiClient.MessageReceived += fct;
        }
    }
}
