using Xunit;
using System;
using SanaraV2.Features.NSFW;
using SanaraV2.Features.Entertainment;
using System.Net;

namespace Sanara_UnitTests
{
    public class Program
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

        [Fact]
        public async void TestAnime()
        {
            var result = await AnimeManga.SearchAnime(true, ("Gochuumon wa Usagi desu ka?").Split(' '));
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
        public async void TestDoujinshi()
        {
            var result = await Doujinshi.SearchDoujinshi(false, new string[] { "color", "english" }, new Random());
            Assert.Equal(SanaraV2.Features.NSFW.Error.Doujinshi.None, result.error);
            Assert.True(IsLinkValid(result.answer.url));
        }

        [Fact]
        public async void TestBooruSafe()
        {
            var result = await Booru.SearchBooru(false, null, new BooruSharp.Booru.Safebooru(), new Random());
            Assert.Equal(SanaraV2.Features.NSFW.Error.Booru.None, result.error);
            Assert.Equal(Discord.Color.Green, result.answer.colorRating);
            Assert.True(IsLinkValid(result.answer.url));
        }

        [Fact]
        public async void TestBooruNotSafe()
        {
            var result = await Booru.SearchBooru(false, new string[] { "sex" }, new BooruSharp.Booru.Gelbooru(), new Random());
            Assert.Equal(SanaraV2.Features.NSFW.Error.Booru.None, result.error);
            Assert.Equal(Discord.Color.Red, result.answer.colorRating);
            Assert.True(IsLinkValid(result.answer.url));
        }
    }
}
