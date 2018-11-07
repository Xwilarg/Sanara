using Xunit;
using SanaraV2.Features.Entertainment;
using System;

namespace Sanara_UnitTests
{
    public class Program
    {
        [Fact]
        public async void TestAnime()
        {
            var result = await AnimeManga.SearchAnime(true, ("Gochuumon wa Usagi desu ka?").Split(' '));
            Assert.Equal(Error.AnimeManga.None, result.error);
            Assert.NotNull(result.answer);
            Assert.Equal("Gochuumon wa Usagi desu ka?", result.answer.name);
            Assert.Equal("https://media.kitsu.io/anime/poster_images/8095/original.jpg?1408463456", result.answer.imageUrl);
            Assert.Equal("GochiUsa", string.Join("", result.answer.alternativeTitles));
            Assert.Equal(12, result.answer.episodeCount);
            Assert.Equal(23, result.answer.episodeLength);
            Assert.InRange(result.answer.rating, 60, 90);
            Assert.Equal(new DateTime(2014, 4, 10), result.answer.startDate);
            Assert.Equal(new DateTime(2014, 6, 26), result.answer.endDate);
            Assert.Equal("Teens 13 or older", result.answer.ageRating);
            Assert.InRange(result.answer.synopsis.Length, 800, 1200);
        }
    }
}
