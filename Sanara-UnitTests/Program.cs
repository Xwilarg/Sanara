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
           Assert.True(LinguistModule.ToKatakana(LinguistModule.FromHiragana("oranji じゅいす")) == "オランジ ジュイス");
        }

        [Fact]
        public void ToHiragana()
        {
            Assert.True(LinguistModule.ToHiragana(LinguistModule.FromKatakana("oranji ジュイス")) == "おらんじ じゅいす");
        }

        [Fact]
        public void ToRomaji()
        {
            Assert.True(LinguistModule.FromHiragana(LinguistModule.FromKatakana("おらんじ ジュイス")) == "oranji juisu");
        }

        [Fact]
        public async void VnDescription()
        {
            Embed e = VndbModule.GetEmbed(await VndbModule.GetVn("hoshizora no memoria wish upon a shooting star"), 0, true);
            Assert.True(e.Title == "星空のメモリア-Wish upon a shooting star- (Hoshizora no Memoria -Wish upon a Shooting Star-)");
            string[] description = e.Description.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.True(description[0] == "availableEnglish");
            Assert.True(description[1] == "availableWindows");
            Assert.True(description[2] == "Long (30 - 50 hours)");
            Assert.True(description[4] == "Released the 27/3/2009.");
            Assert.True(description[5] == "After their mother passes away, Kogasaka You and his younger sister, Chinami, pack up and move out of the city back to their hometown of Hibarigasaki to live with their aunt Shino. Before making the move to the city, You spent almost every single day after school playing with a girl up at the town observation lookout; she was You's first true friend. And when she learns of You’s upcoming relocation to the city she becomes very upset…so much so that she makes him promise to her that one-day he must return to Hibarigasaki to marry her. And with a final departure she kisses You on the forehead.");
            Assert.True(description[6] == "Years later, upon returning to Hibarigasaki, You comes across a now abandoned, fenced-off observatory lookout. It is at this lookout where he encounters a mysterious scythe-wielding girl named Mare who looks strangely like his childhood friend from years past. But as You's life back in his hometown progresses, he is able to makes new friends both in the astronomy club at school and while working at a local restaurant, all while still attempting to seek out his childhood friend. Intrigued, he continues to visit with this mysterious girl at the lookout, but who is she really, and will he ever be to find his childhood friend and make good on his promise?");
            Assert.True(e.Image.HasValue);
        }

        [Fact]
        public async void VnGetImage()
        {
            Directory.GetFiles(".", "vn*").ToList().ForEach(delegate (string path) { if (!path.EndsWith(".dll")) File.Delete(path); });
            List<string> images = VndbModule.GetImages(await VndbModule.GetVn("Rondo Duo Yoake no Fortissimo Punyu Puri ff"), 0, 0, true);
            List<string> results = Directory.GetFiles(".", "vn*").ToList();
            results.RemoveAll(x => x.EndsWith(".dll"));
            Assert.True(results.Count == 1);
        }

        [Fact]
        public async void Booru()
        {
            new SanaraV2.Program(true);
            Directory.GetFiles(".", "booruImage.*").ToList().ForEach(delegate(string path) { File.Delete(path); } );
            await BooruModule.GetImage(new BooruModule.Safebooru(), new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" }, null, "booruImage", true, false);
            Assert.True(Directory.GetFiles(".", "booruImage.*").Length == 1);
        }

        private string[] DownloadBooru(BooruModule.Booru b, string[] tags)
        {
            new SanaraV2.Program(true);
            string url = BooruModule.GetBooruUrl(b, tags);
            Assert.True(url != null);
            return (Utilities.AddArgs(BooruModule.GetTagsInfos(BooruModule.DownloadJson(new WebClient(), url), b, 0).ToArray()).Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None));
        }

        [Fact]
        public void DownloadSafebooru()
        {
            string[] result = DownloadBooru(new BooruModule.Safebooru(), new string[] { "elodie" });
            Assert.True(result.Length >= 2);
            Assert.True(result[0].Contains("Long Live The Queen") && result[1].Contains("Elodie"));
        }

        [Fact]
        public void DownloadGelbooru()
        {
            string[] result = DownloadBooru(new BooruModule.Gelbooru(), new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" });
            Assert.True(result.Length >= 2);
            Assert.True(result[0].Contains("Kantai Collection") && result[1].Contains("Akatsuki") && result[1].Contains("Hibiki"));
        }

        [Fact]
        public void DownloadRule34()
        {
            string[] result = DownloadBooru(new BooruModule.Rule34(), new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" });
            Assert.True(result.Length >= 2);
            Assert.True(result[0].Contains("Kantai Collection") && result[1].Contains("Akatsuki") && result[1].Contains("Hibiki"));
        }

        [Fact]
        public void DownloadKonachan()
        {
            string[] result = DownloadBooru(new BooruModule.Konachan(), new string[] { "hibiki_(kancolle)", "akatsuki_(kancolle)" });
            Assert.True(result.Length >= 2);
            Assert.True(result[0].Contains("Kantai Collection") && result[1].Contains("Akatsuki") && result[1].Contains("Hibiki"));
        }

        [Fact]
        public void DownloadE621()
        {
            string[] result = DownloadBooru(new BooruModule.E621(), new string[] { "shimakaze_(kantai_collection)", "abyssal_(kantai_collection)" });
            Assert.True(result.Length >= 2);
            Assert.True(result[0].Contains("Kantai Collection") && result[1].Contains("Shimakaze") && result[1].Contains("Abyssal"));
        }
    }
}
