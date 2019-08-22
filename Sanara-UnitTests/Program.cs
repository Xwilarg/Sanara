/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.
using Xunit;
using System;
using SanaraV2.Features.NSFW;
using System.Net;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using SanaraV2.Games;
using System.Reflection;

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
            var result = await SanaraV2.Features.Entertainment.AnimeManga.SearchAnime(true, ("Gochuumon wa Usagi desu ka?").Split(' '), null);
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
            Assert.Equal(Error.Doujinshi.None, result.error);
            Assert.True(IsLinkValid(result.answer.url));
            Assert.True(IsLinkValid(result.answer.imageUrl));
            Assert.Contains("full color", result.answer.tags);
            Assert.Contains("english", result.answer.tags);
        }

        [Fact]
        public async Task TestCosplay()
        {
            var result = await Doujinshi.SearchCosplay(false, new string[] { "kantai", "collection" }, new Random());
            Assert.Equal(Error.Doujinshi.None, result.error);
            Assert.True(IsLinkValid(result.answer.url));
            Assert.True(IsLinkValid(result.answer.imageUrl));
            Assert.Contains("kantai collection", result.answer.tags);
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

        // LINGUISTIC
        [Theory]
        [InlineData("neko", "ネコ", "ねこ", "neko")]
        [InlineData("sasayaki", "ササヤキ", "ささやき", "sasayaki")]
        [InlineData("くま", "クマ", "くま", "kuma")]
        [InlineData("ローマじ", "ローマジ", "ろまじ", "romaji")]
        public void TestLinguistic(string original, string katakana, string hiragana, string romaji)
        {
            Assert.Equal(hiragana, SanaraV2.Features.Tools.Linguist.ToHiragana(original));
            Assert.Equal(katakana, SanaraV2.Features.Tools.Linguist.ToKatakana(original));
            Assert.Equal(romaji, SanaraV2.Features.Tools.Linguist.ToRomaji(original));
        }

        [Fact]
        public async Task TestKanji()
        {
            var result = await SanaraV2.Features.Tools.Linguist.Kanji(new[] { "eat" });
            Assert.Equal(SanaraV2.Features.Tools.Error.Kanji.None, result.error);
            Assert.Equal('食', result.answer.kanji);
            Assert.Equal("食 (飠)", result.answer.radicalKanji);
            Assert.Equal("eat, food", result.answer.radicalMeaning);
            Assert.Single(result.answer.parts);
            Assert.True(result.answer.parts.ContainsKey("食"));
            Assert.True(result.answer.parts.ContainsValue("eat, food"));
            Assert.Equal(4, result.answer.onyomi.Count);
            Assert.True(result.answer.onyomi.ContainsKey("く.う"));
            Assert.True(result.answer.onyomi.ContainsValue("ku.u"));
            Assert.True(result.answer.onyomi.ContainsKey("く.らう"));
            Assert.True(result.answer.onyomi.ContainsValue("ku.rau"));
            Assert.True(result.answer.onyomi.ContainsKey("た.べる"));
            Assert.True(result.answer.onyomi.ContainsValue("ta.beru"));
            Assert.True(result.answer.onyomi.ContainsKey("は.む"));
            Assert.True(result.answer.onyomi.ContainsValue("ha.mu"));
            Assert.Equal(2, result.answer.kunyomi.Count);
            Assert.True(result.answer.kunyomi.ContainsKey("ショク"));
            Assert.True(result.answer.kunyomi.ContainsValue("shoku"));
            Assert.True(result.answer.kunyomi.ContainsKey("ジキ"));
            Assert.True(result.answer.kunyomi.ContainsValue("jiki"));
            Assert.Equal("http://classic.jisho.org/static/images/stroke_diagrams/39135_frames.png", result.answer.strokeOrder);
        }

        [Fact]
        public async Task TestKanji2()
        {
            var result = await SanaraV2.Features.Tools.Linguist.Kanji(new[] { "肉" });
            Assert.Equal(SanaraV2.Features.Tools.Error.Kanji.None, result.error);
            Assert.Equal('肉', result.answer.kanji);
            Assert.Equal("肉 (⺼)", result.answer.radicalKanji);
            Assert.Equal("meat", result.answer.radicalMeaning);
            Assert.Equal(3, result.answer.parts.Count);
            Assert.True(result.answer.parts.ContainsKey("肉"));
            Assert.True(result.answer.parts.ContainsValue("meat"));
            Assert.True(result.answer.parts.ContainsKey("人"));
            Assert.True(result.answer.parts.ContainsValue("person"));
            Assert.True(result.answer.parts.ContainsKey("冂"));
            Assert.True(result.answer.parts.ContainsValue("upside-down box radical (no. 13)"));
            Assert.Single(result.answer.onyomi);
            Assert.True(result.answer.onyomi.ContainsKey("しし"));
            Assert.True(result.answer.onyomi.ContainsValue("shishi"));
            Assert.Single(result.answer.kunyomi);
            Assert.True(result.answer.kunyomi.ContainsKey("ニク"));
            Assert.True(result.answer.kunyomi.ContainsValue("niku"));
            Assert.Equal("http://classic.jisho.org/static/images/stroke_diagrams/32905_frames.png", result.answer.strokeOrder);
        }

        // CODE
        [Fact]
        public async Task TestShell()
        {
            var result = await SanaraV2.Features.Tools.Code.Shell(new[] { "ls", "-la" });
            Assert.Equal(SanaraV2.Features.Tools.Error.Shell.None, result.error);
            Assert.Equal("https://explainshell.com/explain?cmd=ls%20-la", result.answer.url);
            Assert.Equal("ls -la", result.answer.title);
            Assert.Equal(3, result.answer.explanations.Count);
            Assert.Equal("ls(1)", result.answer.explanations[0].Item1);
            Assert.Equal("-l", result.answer.explanations[1].Item1);
            Assert.Equal("a", result.answer.explanations[2].Item1);
            Assert.NotEmpty(result.answer.explanations[0].Item2);
            Assert.NotEmpty(result.answer.explanations[1].Item2);
            Assert.NotEmpty(result.answer.explanations[2].Item2);
        }

        [Theory]
        [InlineData(new[] { "255", "0", "0" }, 255, 0, 0, "FF0000", "Red")]
        [InlineData(new[] { "D2B48C" }, 210, 180, 140, "D2B48C", "Tan")]
        public async Task TestColor(string[] original, byte red, byte green, byte blue, string hexa, string name)
        {
            var result = await SanaraV2.Features.Tools.Code.SearchColor(original, new Random());
            Assert.Equal(SanaraV2.Features.Tools.Error.Image.None, result.error);
            Assert.Equal(red, result.answer.discordColor.R);
            Assert.Equal(green, result.answer.discordColor.G);
            Assert.Equal(blue, result.answer.discordColor.B);
            Assert.Equal(hexa, result.answer.colorHex);
            Assert.Equal(name, result.answer.name);
            Assert.True(IsLinkValid(result.answer.colorUrl));
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

        private async Task CheckGame(AGame game)
        {
            Type type = game.GetType();
            MethodInfo info = type.GetMethod("GetPostAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (string s in await (Task<string[]>)info.Invoke(game, null))
            {
                Assert.True(IsLinkValid(s), "Invalid URL " + s);
            }
        }

        [Fact]
        public async Task TestGameAnime()
            => await CheckGame(new SanaraV2.Games.Impl.Anime(null, new Config(0, Difficulty.Normal, "anime", false, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameAzurLane()
            => await CheckGame(new SanaraV2.Games.Impl.AzurLane(null, new Config(0, Difficulty.Normal, "azurlane", false, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameBooru()
            => await CheckGame(new SanaraV2.Games.Impl.Booru(null, new Config(0, Difficulty.Normal, "booru", false, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameFateGO()
            => await CheckGame(new SanaraV2.Games.Impl.FateGO(null, new Config(0, Difficulty.Normal, "fatego", false, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameKanColle()
            => await CheckGame(new SanaraV2.Games.Impl.KanColle(null, new Config(0, Difficulty.Normal, "kancolle", false, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGamePokemon()
            => await CheckGame(new SanaraV2.Games.Impl.Pokemon(null, new Config(0, Difficulty.Normal, "pokemon", false, APreload.Multiplayer.SoloOnly), 0));

        // BOORU MODULE
        [Fact]
        public async Task TestBooruSafe()
        {
            var result = await Booru.SearchBooru(false, null, new BooruSharp.Booru.Safebooru(), new Random());
            Assert.Equal(Error.Booru.None, result.error);
            Assert.Equal(Color.Green, result.answer.colorRating);
            Assert.True(IsLinkValid(result.answer.url));
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


        [Fact]
        public async Task TestBooruTypo()
        {
            BooruSharp.Booru.Gelbooru booru = new BooruSharp.Booru.Gelbooru();
            Random rand = new Random();
            var resultSearch = await Booru.SearchBooru(false, new string[] { "tsutsukakushi_tsuki" }, booru, rand);
            Assert.Equal(Error.Booru.None, resultSearch.error);
            var resultTags = await Booru.SearchTags(new string[] { resultSearch.answer.saveId.ToString() });
            Assert.Equal(Error.BooruTags.None, resultTags.error);
            Assert.Contains("tsutsukakushi_tsukiko", resultTags.answer.characTags);
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
}
