using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            ClassicAssert.AreEqual(answer, Language.ToHiragana(input));
        }
    }
}
