using Xunit;
using SanaraV2;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;

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
        public async void Vn()
        {
            Assert.True((await VndbModule.GetVn("hoshizora no memoria wish upon a shooting star")).Id == 1474);
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
            string[] result = DownloadBooru(new BooruModule.Safebooru(), new string[] { "hibiki_(kantai_collection)", "akatsuki_(kantai_collection)" });
            Assert.True(result.Length >= 2);
            Assert.True(result[0].Contains("Kantai Collection") && result[1].Contains("Akatsuki") && result[1].Contains("Hibiki"));
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
