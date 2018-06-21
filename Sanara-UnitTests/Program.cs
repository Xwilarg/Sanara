using Xunit;
using SanaraV2;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using Discord;
using System;
using System.Collections.Generic;

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
        public async void Booru()
        {
            new SanaraV2.Program(true);
            Directory.GetFiles(".", "booruImage.*").ToList().ForEach(delegate (string path) { File.Delete(path); });
            await BooruModule.GetImage(new BooruModule.Safebooru(), new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" }, null, "booruImage", true, false);
            Assert.Single(Directory.GetFiles(".", "booruImage.*"));
        }

        private string[] DownloadBooru(BooruModule.Booru b, string[] tags)
        {
            new SanaraV2.Program(true);
            string url = BooruModule.GetBooruUrl(b, tags);
            Assert.NotNull(url);
            return (Utilities.AddArgs(BooruModule.GetTagsInfos(BooruModule.DownloadJson(new WebClient(), url), b, 0).ToArray()).Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None));
        }

        [Fact]
        public void DownloadSafebooru()
        {
            string[] result = DownloadBooru(new BooruModule.Safebooru(), new string[] { "elodie" });
            Assert.True(result.Length >= 2);
            Assert.Contains("Long Live The Queen", result[0]);
            Assert.Contains("Elodie", result[1]);
        }

        [Fact]
        public void DownloadGelbooru()
        {
            string[] result = DownloadBooru(new BooruModule.Gelbooru(), new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" });
            Assert.True(result.Length >= 2);
            Assert.Contains("Kantai Collection", result[0]);
            Assert.Contains("Akatsuki", result[1]);
            Assert.Contains("Hibiki", result[1]);
        }

        [Fact]
        public void DownloadRule34()
        {
            string[] result = DownloadBooru(new BooruModule.Rule34(), new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" });
            Assert.True(result.Length >= 2);
            Assert.Contains("Kantai Collection", result[0]);
            Assert.Contains("Akatsuki", result[1]);
            Assert.Contains("Hibiki", result[1]);
        }

        [Fact]
        public void DownloadKonachan()
        {
            string[] result = DownloadBooru(new BooruModule.Konachan(), new string[] { "hibiki_(kancolle)", "akatsuki_(kancolle)" });
            Assert.True(result.Length >= 2);
            Assert.Contains("Kantai Collection", result[0]);
            Assert.Contains("Akatsuki", result[1]);
            Assert.Contains("Hibiki", result[1]);
        }

        [Fact]
        public void DownloadE621()
        {
            string[] result = DownloadBooru(new BooruModule.E621(), new string[] { "shimakaze_(kantai_collection)", "abyssal_(kantai_collection)" });
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
            Assert.False(KancolleModule.GetShipInfos("awawawawawawa", out _, out _));
        }

        [Fact]
        public void ShipInfos()
        {
            string id, thumbnail;
            Assert.True(KancolleModule.GetShipInfos("Ikazuchi", out id, out thumbnail));
            Assert.Equal("2524", id);
            Assert.Equal("https://vignette.wikia.nocookie.net/kancolle/images/e/e6/DD_Ikazuchi_036_Card.jpg", thumbnail);
        }

        [Fact]
        public void DownloadThumbnail()
        {
            string thumbnail;
            Assert.True(KancolleModule.GetShipInfos("Hibiki", out _, out thumbnail));
            string fileName = KancolleModule.DownloadShipThumbnail(thumbnail);
            Assert.True(File.Exists(fileName));
        }

        [Fact]
        public void FillKancolleInfos()
        {
            string id;
            KancolleModule.GetShipInfos("Ryuujou", out id, out _);
            string infos = Utilities.AddArgs(KancolleModule.FillKancolleInfos(id, 0).ToArray());
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
            string[] code = CodeModule.IndenteCode("for (int i = 0; i < 5; i++) { if (i % 2 == 0) a++; }".Split(' ')).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(5, code.Length);
            Assert.Equal("for (int i = 0; i < 5; i++)", code[0]);
            Assert.Equal("{", code[1]);
            Assert.Equal("\tif (i % 2 == 0)", code[2]);
            Assert.Equal("\t\ta++;", code[3]);
            Assert.Equal("}", code[4]);
        }
    }
}
