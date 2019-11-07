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
using System.Threading.Tasks;
using Discord;
using System.Linq;
using SanaraV2.Games;
using System.Reflection;
using System.Net.Http;

namespace Sanara_UnitTests
{
    public class Program : IClassFixture<Tests>
    {
        private static async Task<bool> IsLinkValid(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                using (HttpClient hc = new HttpClient())
                    await hc.SendAsync(new HttpRequestMessage(HttpMethod.Head, new Uri(url)));
                return true;
            }
            return false;
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

        [Fact]
        public async Task TestSourceAnime()
        {
            var result = await SanaraV2.Features.Entertainment.AnimeManga.SearchSource(false, false, null, null, new[] { "https://trace.moe/img/draw2-good.jpg" });
            Assert.Equal(SanaraV2.Features.Entertainment.Error.Source.None, result.error);
            Assert.NotNull(result.answer);
            Assert.Equal("Gochuumon wa Usagi Desu ka??", result.answer.name);
            Assert.False(result.answer.isNsfw);
            Assert.Equal(1, result.answer.episode);
            Assert.Equal("4:51", result.answer.at);
            Assert.Equal("https://trace.moe/thumbnail.php?anilist_id=21034&file=%5BDymy%5D%5BGochuumon%20wa%20Usagi%20Desu%20ka%5D%5BS2%5D%5B01%5D%5BBIG5%5D%5B1280X720%5D.mp4&t=291.08&token=Ffs0TXlswccEj-6Yyg3ALg", result.answer.imageUrl);
            Assert.InRange(result.answer.compatibility, 0.9f, 1f);
        }

        [Fact]
        public async Task TestSourceBooru()
        {
            var result = await Booru.SearchSourceBooru(new[] { "https://konachan.net/sample/8e62b8cf03665480cfe40e71a9cc8797/Konachan.com%20-%20273621%20sample.jpg" });
            Assert.Equal(Error.SourceBooru.None, result.error);
            Assert.NotNull(result.answer);
            Assert.Equal("https://img3.saucenao.com/booru/8/7/87b29dd1740518f2d0394b8d76e31509_1.jpg", result.answer.url);
            float comp = float.Parse(result.answer.compatibility);
            Assert.InRange(comp, 90f, 99f);
            Assert.Contains("Twitter @Calico_Malyu", result.answer.content);
            Assert.Contains("kantai collection", result.answer.content);
        }

        // DOUJINSHI MODULE
        [Fact]
        public async Task TestDoujinshi()
        {
           var result = await Doujinshi.SearchDoujinshi(false, new string[] { "color", "english" }, new Random());
            Assert.Equal(Error.Doujinshi.None, result.error);
            Assert.True(await IsLinkValid(result.answer.url));
            Assert.True(await IsLinkValid(result.answer.imageUrl));
            Assert.Contains("full color", result.answer.tags);
            Assert.Contains("english", result.answer.tags);
        }

        [Fact]
        public async Task TestCosplay()
        {
            var result = await Doujinshi.SearchCosplay(false, new string[] { "kantai", "collection" }, new Random());
            Assert.Equal(Error.Doujinshi.None, result.error);
            Assert.True(await IsLinkValid(result.answer.url));
            Assert.True(await IsLinkValid(result.answer.imageUrl));
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
            Assert.Equal(4, result.answer.kunyomi.Count);
            Assert.True(result.answer.kunyomi.ContainsKey("く.う"));
            Assert.True(result.answer.kunyomi.ContainsValue("ku.u"));
            Assert.True(result.answer.kunyomi.ContainsKey("く.らう"));
            Assert.True(result.answer.kunyomi.ContainsValue("ku.rau"));
            Assert.True(result.answer.kunyomi.ContainsKey("た.べる"));
            Assert.True(result.answer.kunyomi.ContainsValue("ta.beru"));
            Assert.True(result.answer.kunyomi.ContainsKey("は.む"));
            Assert.True(result.answer.kunyomi.ContainsValue("ha.mu"));
            Assert.Equal(2, result.answer.onyomi.Count);
            Assert.True(result.answer.onyomi.ContainsKey("ショク"));
            Assert.True(result.answer.onyomi.ContainsValue("shoku"));
            Assert.True(result.answer.onyomi.ContainsKey("ジキ"));
            Assert.True(result.answer.onyomi.ContainsValue("jiki"));
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
            Assert.True(await IsLinkValid(result.answer.colorUrl));
        }

        // GAMES MODULE
        [Fact]
        public void TestGames()
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
                Assert.True(await IsLinkValid(s), "Invalid URL " + s);
            }
        }

        [Fact]
        public async Task TestGameAnime()
            => await CheckGame(new SanaraV2.Games.Impl.Anime(null, new Config(0, Difficulty.Normal, "anime", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameAzurLane()
            => await CheckGame(new SanaraV2.Games.Impl.AzurLane(null, new Config(0, Difficulty.Normal, "azurlane", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameBooru()
            => await CheckGame(new SanaraV2.Games.Impl.Booru(null, new Config(0, Difficulty.Normal, "booru", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameFateGO()
            => await CheckGame(new SanaraV2.Games.Impl.FateGO(null, new Config(0, Difficulty.Normal, "fatego", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameKanColle()
            => await CheckGame(new SanaraV2.Games.Impl.KanColle(null, new Config(0, Difficulty.Normal, "kancolle", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGamePokemon()
            => await CheckGame(new SanaraV2.Games.Impl.Pokemon(null, new Config(0, Difficulty.Normal, "pokemon", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0));

        [Fact]
        public async Task TestGameGirlsFrontline()
            => await CheckGame(new SanaraV2.Games.Impl.GirlsFrontline(null, new Config(0, Difficulty.Normal, "girlsfrontline", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0));

        [Theory]
        [InlineData("Kisaragi", "https://azurlane.koumakan.jp/w/images/thumb/b/ba/Kisaragi.png/600px-Kisaragi.png")]
        [InlineData("Li%27l_Sandy", "https://azurlane.koumakan.jp/w/images/thumb/1/19/Li%27l_Sandy.png/600px-Li%27l_Sandy.png")]
        [InlineData("33", "https://azurlane.koumakan.jp/w/images/thumb/9/9c/33.png/600px-33.png")]
        [InlineData("Le_Temeraire", "https://azurlane.koumakan.jp/w/images/thumb/9/94/Le_Temeraire.png/600px-Le_Temeraire.png")]
        [InlineData("Ibuki", "https://azurlane.koumakan.jp/w/images/thumb/7/75/Ibuki.png/600px-Ibuki.png")]
        [InlineData("Laffey", "https://azurlane.koumakan.jp/w/images/thumb/2/2a/Laffey.png/595px-Laffey.png")]
        public async Task TestAzurLaneDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.azurLaneDictionnary);
            var game = new SanaraV2.Games.Impl.AzurLane(null, new Config(0, Difficulty.Normal, "azurlane", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("Mashu_Kyrielight", "https://vignette.wikia.nocookie.net/fategrandorder/images/b/b0/Shielder1.png")]
        [InlineData("Tamamo_Cat", "https://vignette.wikia.nocookie.net/fategrandorder/images/f/fd/Tamamo01.png")]
        [InlineData("Miyamoto_Musashi_(Berserker)", "https://vignette.wikia.nocookie.net/fategrandorder/images/8/85/Musashi_Berserker_1.png")]
        [InlineData("Sakamoto_Ry%C5%8Dma", "https://vignette.wikia.nocookie.net/fategrandorder/images/9/97/Ryoma_1.png")]
        public async Task TestFateGODictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.fateGODictionnary);
            var game = new SanaraV2.Games.Impl.FateGO(null, new Config(0, Difficulty.Normal, "fatego", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("Ryuujou", "https://vignette.wikia.nocookie.net/kancolle/images/8/81/Ryuujou_Full.png")]
        [InlineData("Commandant_Teste", "https://vignette.wikia.nocookie.net/kancolle/images/c/ca/Commandant_Teste_Full.png")]
        [InlineData("Maruyu", "https://vignette.wikia.nocookie.net/kancolle/images/a/aa/Maruyu_Full.png")]
        public async Task TestKanColleDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.kanColleDictionnary);
            var game = new SanaraV2.Games.Impl.KanColle(null, new Config(0, Difficulty.Normal, "kancolle", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("ponyta", "https://img.pokemondb.net/artwork/ponyta.jpg")]
        [InlineData("farfetchd", "https://img.pokemondb.net/artwork/farfetchd.jpg")]
        [InlineData("solgaleo", "https://img.pokemondb.net/artwork/solgaleo.jpg")]
        public async Task TestPokemonDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.pokemonDictionnary);
            var game = new SanaraV2.Games.Impl.Pokemon(null, new Config(0, Difficulty.Normal, "pokemon", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("FNC", "https://en.gfwiki.com/images/thumb/e/ec/FNC.png/600px-FNC.png")]
        [InlineData("OTs-14", "https://en.gfwiki.com/images/thumb/9/93/OTs-14.png/600px-OTs-14.png")]
        [InlineData("ST_AR-15", "https://en.gfwiki.com/images/thumb/2/2e/ST_AR-15.png/600px-ST_AR-15.png")]
        public async Task TestGirlsFrontlineDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.girlsfrontlineDictionnary);
            var game = new SanaraV2.Games.Impl.GirlsFrontline(null, new Config(0, Difficulty.Normal, "girlsfrontline", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        // BOORU MODULE
        [Fact]
        public async Task TestBooruSafe()
        {
            var result = await Booru.SearchBooru(false, null, new BooruSharp.Booru.Safebooru(), new Random());
            Assert.Equal(Error.Booru.None, result.error);
            Assert.Equal(Color.Green, result.answer.colorRating);
            Assert.True(await IsLinkValid(result.answer.url));
        }

        [Fact]
        public async Task TestBooruNotSafe()
        {
            var result = await Booru.SearchBooru(false, new string[] { "cum_in_pussy" }, new BooruSharp.Booru.Gelbooru(), new Random());
            Assert.Equal(Error.Booru.None, result.error);
            Assert.Equal(Color.Red, result.answer.colorRating);
            Assert.True(await IsLinkValid(result.answer.url));
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
                Assert.True(IsLinkValid(img.Value.Url).GetAwaiter().GetResult());
                msgReceived = true;
                return Task.CompletedTask;
            });
            while (!msgReceived) { }
        }
    }
}
