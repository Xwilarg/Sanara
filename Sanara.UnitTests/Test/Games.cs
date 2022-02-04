using NUnit.Framework;
using Sanara.Module.Utility;

namespace Sanara.UnitTests.Test
{
    public class Games
    {
        [TestCase("gyuunyuu", "ぎゅうにゅう")]
        [TestCase("gakkou", "がっこう")]
        [TestCase("ちち", "ちち")]
        [TestCase("ラムネ", "らむね")]
        public async Task TestHiraganaConvertion(string input, string answer)
        {
            Assert.AreEqual(answer, Language.ToHiragana(input));
        }
    }
}
