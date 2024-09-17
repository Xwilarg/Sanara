using NUnit.Framework;
using NUnit.Framework.Legacy;
using Sanara.Service;

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
            var converter = new JapaneseConverter();
            ClassicAssert.AreEqual(answer, converter.ToHiragana(input));
        }
    }
}
