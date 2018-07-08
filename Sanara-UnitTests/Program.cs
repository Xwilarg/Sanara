using Xunit;
using SanaraV2;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using Discord;
using System;
using System.Collections.Generic;
using SanaraV2.GamesInfo;
using SanaraV2.NSFW;
using SanaraV2.Base;
using SanaraV2.Entertainment;
using SanaraV2.Tools;

namespace Sanara_UnitTests
{
    public class Program
    {
        [Fact]
        public void ToKatakana()
        {
            Assert.Equal("オランジ ジュイス", LinguistModule.ToKatakana(LinguistModule.FromHiragana("oranji じゅいす")));
        }

        [Fact]
        public void ToHiragana()
        {
            Assert.Equal("おらんじ じゅいす", LinguistModule.ToHiragana(LinguistModule.FromKatakana("oranji ジュイス")));
        }

        [Fact]
        public void ToRomaji()
        {
            Assert.Equal("oranji juisu", LinguistModule.FromHiragana(LinguistModule.FromKatakana("おらんじ ジュイス")));
        }

        [Fact]
        public async void VnDescription()
        {
            Embed e = VnModule.GetEmbed(await VnModule.GetVn("hoshizora no memoria wish upon a shooting star"), 0, true);
            Assert.True(e.Title == "星空のメモリア-Wish upon a shooting star- (Hoshizora no Memoria -Wish upon a Shooting Star-)");
            string[] description = e.Description.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal("availableEnglish", description[0]);
            Assert.Equal("availableWindows", description[1]);
            Assert.Equal("Long (30 - 50 hours)", description[2]);
            Assert.Equal("Released the 27/3/2009.", description[4]);
            Assert.Equal("After their mother passes away, Kogasaka You and his younger sister, Chinami, pack up and move out of the city back to their hometown of Hibarigasaki to live with their aunt Shino. Before making the move to the city, You spent almost every single day after school playing with a girl up at the town observation lookout; she was You's first true friend. And when she learns of You’s upcoming relocation to the city she becomes very upset…so much so that she makes him promise to her that one-day he must return to Hibarigasaki to marry her. And with a final departure she kisses You on the forehead.", description[5]);
            Assert.Equal("Years later, upon returning to Hibarigasaki, You comes across a now abandoned, fenced-off observatory lookout. It is at this lookout where he encounters a mysterious scythe-wielding girl named Mare who looks strangely like his childhood friend from years past. But as You's life back in his hometown progresses, he is able to makes new friends both in the astronomy club at school and while working at a local restaurant, all while still attempting to seek out his childhood friend. Intrigued, he continues to visit with this mysterious girl at the lookout, but who is she really, and will he ever be to find his childhood friend and make good on his promise?", description[6]);
            Assert.True(e.Image.HasValue);
        }

        [Fact]
        public async void VnGetImage()
        {
            Directory.GetFiles(".", "vn*").ToList().ForEach(delegate (string path) { if (!path.EndsWith(".dll")) File.Delete(path); });
            List<string> images = VnModule.GetImages(await VnModule.GetVn("Rondo Duo Yoake no Fortissimo Punyu Puri ff"), 0, 0, true);
            List<string> results = Directory.GetFiles(".", "vn*").ToList();
            results.RemoveAll(x => x.EndsWith(".dll"));
            Assert.Single(results);
        }

        [Fact]
        public void DownloadSafebooru()
        {
            BooruModule.Safebooru b = new BooruModule.Safebooru();
            string json;
            BooruModule.GetImageUrl(b, new string[] { "elodie" }, out json);
            string[] result = BooruModule.GetTagsInfos(json, b, 0).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(result.Length >= 2);
            Assert.Contains("Long Live The Queen", result[0]);
            Assert.Contains("Elodie", result[1]);
        }

        [Fact]
        public void DownloadGelbooru()
        {
            BooruModule.Gelbooru b = new BooruModule.Gelbooru();
            string json;
            BooruModule.GetImageUrl(b, new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" }, out json);
            string[] result = BooruModule.GetTagsInfos(json, b, 0).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(result.Length >= 2);
            Assert.Contains("Kantai Collection", result[0]);
            Assert.Contains("Akatsuki", result[1]);
            Assert.Contains("Hibiki", result[1]);
        }

        [Fact]
        public void DownloadRule34()
        {
            BooruModule.Rule34 b = new BooruModule.Rule34();
            string json;
            BooruModule.GetImageUrl(b, new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" }, out json);
            string[] result = BooruModule.GetTagsInfos(json, b, 0).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(result.Length >= 2);
            Assert.Contains("Kantai Collection", result[0]);
            Assert.Contains("Akatsuki", result[1]);
            Assert.Contains("Hibiki", result[1]);
        }

        [Fact]
        public void DownloadKonachan()
        {
            BooruModule.Konachan b = new BooruModule.Konachan();
            string json;
            BooruModule.GetImageUrl(b, new string[] { "hibiki_(kancolle)", "akatsuki_(kancolle)" }, out json);
            string[] result = BooruModule.GetTagsInfos(json, b, 0).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.Contains("Kantai Collection", result[0]);
            Assert.Contains("Akatsuki", result[1]);
            Assert.Contains("Hibiki", result[1]);
        }

        [Fact]
        public void DownloadE621()
        {
            BooruModule.E621 b = new BooruModule.E621();
            string json;
            BooruModule.GetImageUrl(b, new string[] { "shimakaze_(kantai_collection)", "abyssal_(kantai_collection)" }, out json);
            string[] result = BooruModule.GetTagsInfos(json, b, 0).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(result.Length >= 2);
            Assert.Contains("Kantai Collection", result[0]);
            Assert.Contains("Shimakaze", result[1]);
            Assert.Contains("Abyssal", result[1]);
        }

        [Fact]
        public void MapInvalidEmpty()
        {
            Assert.True(KancolleModule.IsMapInvalid(new string[] { }));
        }

        [Fact]
        public void MapInvalidBadWorld()
        {
            Assert.True(KancolleModule.IsMapInvalid(new string[] { "9", "2" }));
        }

        [Fact]
        public void MapInvalidBadLevel()
        {
            Assert.True(KancolleModule.IsMapInvalid(new string[] { "2", "9" }));
        }

        [Fact]
        public void MapValid()
        {
            Assert.False(KancolleModule.IsMapInvalid(new string[] { "2", "2" }));
        }

        [Fact]
        public void GetBranchingRules()
        {
            string mapIntro, mapDraw, mapName;
            string infos = KancolleModule.GetMapInfos('2', '2', out mapIntro, out mapDraw, out mapName);
            Assert.True(mapName == "Bashi Island");
            Assert.True(File.Exists(mapIntro));
            Assert.True(File.Exists(mapDraw));
            string branchs = KancolleModule.GetBranchingRules(infos);
            Assert.Contains("0 -> A,E/resource", branchs);
            Assert.Contains("A -> B/resource,E/resource", branchs);
            Assert.Contains("B/resource -> C/resource,D", branchs);
            Assert.Contains("E/resource -> F/battle", branchs);
            Assert.Contains("E/resource -> G/battle", branchs);
        }

        [Fact]
        public void InvalidShipName()
        {
            Assert.Null(KancolleModule.GetShipName(new string[] { "awawawawawawawa" }));
        }

        [Fact]
        public void InvalidDropConstruction()
        {
            Assert.Null(KancolleModule.GetDropConstruction(KancolleModule.GetShipName(new string[] { "u511" }), 0));
        }

        [Fact]
        public void DropConstruction()
        {
            Assert.NotNull(KancolleModule.GetDropConstruction(KancolleModule.GetShipName(new string[] { "Akitsu", "Maru" }), 0));
        }

        [Fact]
        public void InvalidDropMap()
        {
            KancolleModule.DropMapError error;
            KancolleModule.GetDropMap(KancolleModule.GetShipName(new string[] { "Taihou" }), 0, out error);
            Assert.Equal(KancolleModule.DropMapError.DontDrop, error);
        }

        [Fact]
        public void DropMap()
        {
            KancolleModule.DropMapError error;
            KancolleModule.GetDropMap(KancolleModule.GetShipName(new string[] { "Ikazuchi" }), 0, out error);
            Assert.Equal(KancolleModule.DropMapError.NoError, error);
        }

        [Fact]
        public void WrongShipInfos()
        {
            Assert.Null(Wikia.GetCharacInfos("awawawawawawa", Wikia.WikiaType.KanColle));
        }

        [Fact]
        public void ShipInfos()
        {
            Wikia.CharacInfo? infos = Wikia.GetCharacInfos("Ikazuchi", Wikia.WikiaType.KanColle);
            Assert.NotNull(infos);
            Assert.Equal("2524", infos.Value.id);
            Assert.Equal("https://vignette.wikia.nocookie.net/kancolle/images/e/e6/DD_Ikazuchi_036_Card.jpg", infos.Value.thumbnail);
        }

        [Fact]
        public void TDollInfos()
        {
            Wikia.CharacInfo? infos = Wikia.GetCharacInfos("MP5", Wikia.WikiaType.GirlsFrontline);
            Assert.NotNull(infos);
            Assert.Equal("879", infos.Value.id);
            Assert.Equal("https://vignette.wikia.nocookie.net/girlsfrontline/images/1/16/Mp5_norm.png", infos.Value.thumbnail);
        }

        [Fact]
        public void DownloadThumbnail()
        {
            Wikia.CharacInfo? infos = Wikia.GetCharacInfos("Hibiki", Wikia.WikiaType.KanColle);
            Assert.NotNull(infos);
            string fileName = Wikia.DownloadCharacThumbnail(infos.Value.thumbnail);
            Assert.True(File.Exists(fileName));
        }

        [Fact]
        public void FillKancolleInfos()
        {
            Wikia.CharacInfo? cinfos = Wikia.GetCharacInfos("Ryuujou", Wikia.WikiaType.KanColle);
            Assert.NotNull(cinfos);
            string infos = Utilities.AddArgs(KancolleModule.FillKancolleInfos(cinfos.Value.id, 0).ToArray());
            Assert.Contains("**personality**", infos);
            Assert.Contains("**appearance**", infos);
            Assert.Contains("**trivia**", infos);
        }

        [Fact]
        public void IsLinkValid()
        {
            Assert.True(Utilities.IsLinkValid("http://www.google.com"));
        }

        [Fact]
        public void Doujinshi()
        {
            new SanaraV2.Program(true);
            string url = DoujinshiModule.GetDoujinshi(new string[] { }, out _);
            Assert.NotNull(url);
            Assert.True(Utilities.IsLinkValid(url));
        }

        [Fact]
        public void DoujinshiInvalid()
        {
            new SanaraV2.Program(true);
            string url = DoujinshiModule.GetDoujinshi(new string[] { "awawawawawawawa" }, out _);
            Assert.Null(url);
        }

        [Fact]
        public void Tags()
        {
            new SanaraV2.Program(true);
            string url = DoujinshiModule.GetDoujinshi(new string[] { "kantai", "collection", "color" }, out _);
            Assert.NotNull(url); /// TODO: Sometimes null
            Assert.True(Utilities.IsLinkValid(url));
        }

        [Fact]
        public void Definition()
        {
            Assert.Contains("そら", Utilities.AddArgs(LinguistModule.GetAllKanjis("sky", 0).ToArray()));
        }

        [Fact]
        public void Code()
        {
            string[] code = CodeModule.IndenteCode(CodeModule.ParseCode("for (int i = 0; i < 5; i++) { if (i % 2 == 0) a++; }".Split(' '))).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(5, code.Length);
            Assert.Equal("for (int i = 0; i < 5; i++)", code[0]);
            Assert.Equal("{", code[1]);
            Assert.Equal("\tif (i % 2 == 0)", code[2]);
            Assert.Equal("\t\ta++;", code[3]);
            Assert.Equal("}", code[4]);
        }

        private void InitShiritoriGame()
        {
            if (!Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");
            File.WriteAllText("Saves/shiritoriWords.dat", "りゅう$Dragon");
        }

        [Fact]
        public void GetPostShiritori()
        {
            InitShiritoriGame();
            new SanaraV2.Program(true);
            GameModule.Shiritori shiritori = new GameModule.Shiritori(null, null, null, false);
            Assert.Equal("しりとり (shiritori)", shiritori.GetPost(out _)[0]);
            Assert.Equal("りゅう (ryuu) - Meaning: Dragon", shiritori.GetPost(out _)[0]);
        }

        [Fact]
        public void GetPostKancolle()
        {
            new SanaraV2.Program(true);
            GameModule.Kancolle kancolle = new GameModule.Kancolle(null, null, null, false);
            Assert.True(File.Exists(kancolle.GetPost(out _)[0]));
        }

        [Fact]
        public void GetPostBooru()
        {
            if (!Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");
            File.WriteAllText("Saves/BooruTriviaTags.dat", "swimsuit 10");
            new SanaraV2.Program(true);
            GameModule.BooruGame booru = new GameModule.BooruGame(null, null, null, false);
            foreach (string s in booru.GetPost(out _))
                Assert.True(File.Exists(s));
        }

        [Fact]
        public void CheckCorrectShiritoriCorrect()
        {
            InitShiritoriGame();
            GameModule.Shiritori shiritori = new GameModule.Shiritori(null, null, null, false);
            shiritori.GetPost(out _);
            Assert.Null(shiritori.GetCheckCorrect("ryuu", out _));
        }
    }
}
