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
using System.Collections.Generic;
using System.IO;
using SanaraV2.Subscription;

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

        // SUBSCRIPTION
        [Theory]
        [InlineData(typeof(AnimeSubscription))]
        [InlineData(typeof(NHentaiSubscription))]
        public async Task TestSubscription(Type t)
        {
            var sub = (ASubscription)Activator.CreateInstance(t);
            Assert.NotEqual(0, sub.GetCurrent());
            await sub.SetCurrent(0);
            var datas = await sub.GetFeed();
            Assert.NotEmpty(datas);
            var anime = datas[0];
            Assert.True(await IsLinkValid(anime.Item2.Url));
            Assert.True(await IsLinkValid(anime.Item2.ThumbnailUrl));
            Assert.NotEmpty(anime.Item2.Title);
        }

        // ANIME/MANGA MODULE
        [Fact]
        public async Task TestAnime()
        {
            var result = await SanaraV2.Features.Entertainment.AnimeManga.SearchAnime(SanaraV2.Features.Entertainment.AnimeManga.SearchType.Anime, ("Gochuumon wa Usagi desu ka?").Split(' '), null);
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
            var result = await Booru.SearchSourceBooru(new[] { "https://katdon.files.wordpress.com/2015/10/gochuumon.png" });
            Assert.Equal(Error.SourceBooru.None, result.error);
            Assert.NotNull(result.answer);
            Assert.True(await IsLinkValid(result.answer.url));
            Assert.InRange(result.answer.compatibility, 80f, 99f);
            Assert.Contains("ご注文はうさぎですか", result.answer.content);
        }

        [Fact]
        public async Task TestSourceBooru()
        {
            var result = await Booru.SearchSourceBooru(new[] { "https://konachan.net/sample/8e62b8cf03665480cfe40e71a9cc8797/Konachan.com%20-%20273621%20sample.jpg" });
            Assert.Equal(Error.SourceBooru.None, result.error);
            Assert.NotNull(result.answer);
            Assert.Equal("https://img3.saucenao.com/booru/8/7/87b29dd1740518f2d0394b8d76e31509_1.jpg", result.answer.url);
            Assert.InRange(result.answer.compatibility, 90f, 99f);
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

        [Fact]
        public async Task TestDownloadDoujinshi()
        {
            var result = await Doujinshi.SearchDownloadDoujinshi(false, new[] { "112731" }, () => { return Task.CompletedTask; });
            Assert.Equal(Error.Download.None, result.error);
            Assert.True(Directory.Exists(result.answer.directoryPath));
            Assert.True(File.Exists(result.answer.filePath));
            Assert.Equal("112731", result.answer.id);
        }

        [Fact]
        public async Task TestDownloadCosplay()
        {
            var result = await Doujinshi.SearchDownloadCosplay(false, new[] { "656554/5329651869" }, () => { return Task.CompletedTask; });
            Assert.Equal(Error.Download.None, result.error);
            Assert.True(Directory.Exists(result.answer.directoryPath));
            Assert.True(File.Exists(result.answer.filePath));
            Assert.Equal("656554", result.answer.id);
        }

        [Fact]
        public async Task TestDoujinshiPopularityTags()
        {
            var result = await Doujinshi.SearchDoujinshiByPopularity(false, new[] { "futanari" });
            Assert.Equal(Error.Doujinshi.None, result.error);
            Assert.Equal(5, result.answer.Length);
            foreach (var doujin in result.answer)
            {
                Assert.True(await IsLinkValid(doujin.url));
                Assert.True(await IsLinkValid(doujin.imageUrl));
                Assert.Contains("futanari", doujin.tags);
            }
        }

        [Fact]
        public async Task TestDoujinshiPopularity()
        {
            var result = await Doujinshi.SearchDoujinshiRecentPopularity(false);
            Assert.Equal(Error.Doujinshi.None, result.error);
            Assert.Equal(5, result.answer.Length);
            foreach (var doujin in result.answer)
            {
                Assert.True(await IsLinkValid(doujin.url));
                Assert.True(await IsLinkValid(doujin.imageUrl));
                Assert.Contains("futanari", doujin.tags);
            }
        }

        [Fact]
        public async Task TestAdultVideo()
        {
            var result = await Doujinshi.SearchAdultVideo(false, new string[] { "lesbian" }, new Random(), new List<string>() { "lesbian" });
            Assert.Equal(Error.Doujinshi.None, result.error);
            Assert.True(await IsLinkValid(result.answer.url));
            Assert.True(await IsLinkValid(result.answer.imageUrl));
            Assert.Contains("Lesbian", result.answer.tags);
        }

        // GAMES INFO
        [Theory]
        [InlineData("Ifrit", "Ifrit", "Attacks cause super-distance group spell damage", "https://aceship.github.io/AN-EN-Tags/img/characters/char_134_ifrit_1.png", "Ranged",
            new[] { "Aoe", "Debuff" }, new[] { "Fanaticism", "Sunburst", "Burning Ground" }, new[] {
                "Attack +0.1%, Attack speed +45",
                "The next attack deals 1.3% magical damage. Additionally, inflict -100 Defense and burn the target for 3 seconds. Can hold 2 charges",
                "Deal 0.75% magical damage to enemies on the ground within attack range every second and inflict -7 Magic resistance. However, Ifrit loses 0.02% Max HP every second"
            })]
        [InlineData("Popukar", "Popukar", "", "https://aceship.github.io/AN-EN-Tags/img/characters/char_281_popka_1.png", "Melee",
            new[] { "Aoe", "Survival" }, new[] { "Attack Strengthening·Type α" }, new[] {
                "Attack +0.1%"
            })]
        [InlineData("Gum", "Гум", "Skills can treat friendly units", "https://aceship.github.io/AN-EN-Tags/img/characters/char_196_sunbr_1.png", "Melee",
            new[] { "Defense", "Healing" }, new[] { "Reserve Rations", "Food Preparation" }, new[] {
                "The next attack will heal a nearby ally for 0.95% of ГУМ's Attack. Can hold 1 charges",
                "Begins cooking and stops attacking enemies for 10 seconds, Defense +0.5%. After finishing cooking, focuses on healing nearby allies (attack interval +1.3%), Attack +0.3%"
            })]
        [InlineData("W", "W", "", "https://aceship.github.io/AN-EN-Tags/img/characters/char_113_cqbw_1.png", "Ranged",
            new[] { "Dps", "Crowdcontrol" }, new[] { "K of Hearts", "Jack-in-the-box", "D12" }, new[] {
                "Immediately fire a grenade, dealing 2.3% physical damage and stunning the targets for 1.5 seconds",
                "The next attack changes to burying a landmine on an eligible tile within attack range (lasts for 120 seconds). Landmines trigger when an enemy passes over, dealing 1.9% physical damage to all nearby enemies and stunning them for 1.4 seconds",
                "Plant bombs on up to 3 targets with the highest HP within attack range. The bombs will explode after a delay, each dealing 2.2% physical damage to all nearby enemies and stunning them for 3 seconds"
            })]
        public async Task TestArknights(string title, string name, string description, string imgUrl, string type, string[] tags, string[] skillNames, string[] skillDescription)
        {
            await new SanaraV2.Program().InitArknightsDictionnary();
            var result = await SanaraV2.Features.GamesInfo.Arknights.SearchCharac(new string[] { title });
            Assert.Equal(SanaraV2.Features.GamesInfo.Error.Charac.None, result.error);
            Assert.Equal(name, result.answer.name);
            Assert.Equal(description, result.answer.description);
            Assert.Equal(imgUrl, result.answer.imgUrl);
            Assert.Equal(type, result.answer.type);
            Assert.True(await IsLinkValid(result.answer.wikiUrl));
            foreach (var elem in result.answer.tags)
                Assert.Contains(elem, tags);
            foreach (var elem in result.answer.skills)
            {
                Assert.Contains(elem.name, skillNames);
                Assert.Contains(elem.description, skillDescription);
            }
        }

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
            Assert.Single(result.answer.kunyomi);
            Assert.True(result.answer.kunyomi.ContainsKey("しし"));
            Assert.True(result.answer.kunyomi.ContainsValue("shishi"));
            Assert.Single(result.answer.onyomi);
            Assert.True(result.answer.onyomi.ContainsKey("ニク"));
            Assert.True(result.answer.onyomi.ContainsValue("niku"));
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
            => await CheckGame(new SanaraV2.Games.Impl.Anime(null, null, new Config(0, Difficulty.Normal, "anime", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0));

        [Fact]
        public async Task TestGameAzurLane()
            => await CheckGame(new SanaraV2.Games.Impl.AzurLane(null, null, new Config(0, Difficulty.Normal, "azurlane", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0));

        [Fact]
        public async Task TestGameBooru()
            => await CheckGame(new SanaraV2.Games.Impl.Booru(null, null, new Config(0, Difficulty.Normal, "booru", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0));

        [Fact]
        public async Task TestGameFateGO()
            => await CheckGame(new SanaraV2.Games.Impl.FateGO(null, null, new Config(0, Difficulty.Normal, "fatego", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0));

        [Fact]
        public async Task TestGameKanColle()
            => await CheckGame(new SanaraV2.Games.Impl.KanColle(null, null, new Config(0, Difficulty.Normal, "kancolle", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0));

        [Fact]
        public async Task TestGamePokemon()
            => await CheckGame(new SanaraV2.Games.Impl.Pokemon(null, null, new Config(0, Difficulty.Normal, "pokemon", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0));

        [Fact]
        public async Task TestGameGirlsFrontline()
            => await CheckGame(new SanaraV2.Games.Impl.GirlsFrontline(null, null, new Config(0, Difficulty.Normal, "girlsfrontline", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0));

        [Fact]
        public async Task TestGameArknights()
            => await CheckGame(new SanaraV2.Games.Impl.Arknights(null, null, new Config(0, Difficulty.Normal, "arknights", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0));

        [Theory]
        [InlineData("Kisaragi", "https://azurlane.koumakan.jp/w/images/thumb/b/ba/Kisaragi.png/[0-9]{3}px-Kisaragi.png")]
        [InlineData("Li%27l_Sandy", "https://azurlane.koumakan.jp/w/images/thumb/1/19/Li%27l_Sandy.png/[0-9]{3}px-Li%27l_Sandy.png")]
        [InlineData("33", "https://azurlane.koumakan.jp/w/images/thumb/9/9c/33.png/[0-9]{3}px-33.png")]
        [InlineData("Le_Temeraire", "https://azurlane.koumakan.jp/w/images/thumb/9/94/Le_Temeraire.png/[0-9]{3}px-Le_Temeraire.png")]
        [InlineData("Ibuki", "https://azurlane.koumakan.jp/w/images/thumb/7/75/Ibuki.png/[0-9]{3}px-Ibuki.png")]
        [InlineData("Laffey", "https://azurlane.koumakan.jp/w/images/thumb/2/2a/Laffey.png/[0-9]{3}px-Laffey.png")]
        public async Task TestAzurLaneDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.azurLaneDictionnary);
            var game = new SanaraV2.Games.Impl.AzurLane(null, null, new Config(0, Difficulty.Normal, "azurlane", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0);
            Assert.Matches(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("Mashu_Kyrielight", "https://vignette.wikia.nocookie.net/fategrandorder/images/b/b0/Shielder1.png")]
        [InlineData("Tamamo_Cat", "https://vignette.wikia.nocookie.net/fategrandorder/images/f/fd/Tamamo01.png")]
        [InlineData("Miyamoto_Musashi_(Berserker)", "https://vignette.wikia.nocookie.net/fategrandorder/images/8/85/Musashi_Berserker_1.png")]
        [InlineData("Sakamoto_Ry%C5%8Dma", "https://vignette.wikia.nocookie.net/fategrandorder/images/9/97/Ryoma_1.png")]
        public async Task TestFateGODictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.fateGODictionnary);
            var game = new SanaraV2.Games.Impl.FateGO(null, null, new Config(0, Difficulty.Normal, "fatego", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("Ryuujou", "https://vignette.wikia.nocookie.net/kancolle/images/8/81/Ryuujou_Full.png")]
        [InlineData("Commandant_Teste", "https://vignette.wikia.nocookie.net/kancolle/images/c/ca/Commandant_Teste_Full.png")]
        [InlineData("Maruyu", "https://vignette.wikia.nocookie.net/kancolle/images/a/aa/Maruyu_Full.png")]
        public async Task TestKanColleDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.kanColleDictionnary);
            var game = new SanaraV2.Games.Impl.KanColle(null, null, new Config(0, Difficulty.Normal, "kancolle", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("ponyta", "https://img.pokemondb.net/artwork/ponyta.jpg")]
        [InlineData("farfetchd", "https://img.pokemondb.net/artwork/farfetchd.jpg")]
        [InlineData("solgaleo", "https://img.pokemondb.net/artwork/solgaleo.jpg")]
        public async Task TestPokemonDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.pokemonDictionnary);
            var game = new SanaraV2.Games.Impl.Pokemon(null, null, new Config(0, Difficulty.Normal, "pokemon", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("FNC", "https://en.gfwiki.com/images/thumb/e/ec/FNC.png/600px-FNC.png")]
        [InlineData("OTs-14", "https://en.gfwiki.com/images/thumb/9/93/OTs-14.png/600px-OTs-14.png")]
        [InlineData("ST_AR-15", "https://en.gfwiki.com/images/thumb/2/2e/ST_AR-15.png/600px-ST_AR-15.png")]
        public async Task TestGirlsFrontlineDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.girlsfrontlineDictionnary);
            var game = new SanaraV2.Games.Impl.GirlsFrontline(null, null, new Config(0, Difficulty.Normal, "girlsfrontline", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0);
            Assert.Equal(url, await game.GetUrlTest(name));
        }

        [Theory]
        [InlineData("char_180_amgoat", "https://aceship.github.io/AN-EN-Tags/img/characters/char_180_amgoat_1.png")]
        [InlineData("char_128_plosis", "https://aceship.github.io/AN-EN-Tags/img/characters/char_128_plosis_1.png")]
        [InlineData("char_144_red", "https://aceship.github.io/AN-EN-Tags/img/characters/char_144_red_1.png")]
        public async Task TestArknightsDictionnary(string name, string url)
        {
            Assert.Contains(name, Constants.arknightsDictionnary);
            var game = new SanaraV2.Games.Impl.Arknights(null, null, new Config(0, Difficulty.Normal, "arknights", false, false, false, APreload.Shadow.None, APreload.Multiplayer.SoloOnly, APreload.MultiplayerType.None), 0);
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
