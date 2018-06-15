using Xunit;
using SanaraV2;

namespace Sanara_UnitTests
{
    public class Program
    {
        [Fact]
        public void TLinguisticModule()
        {
           Assert.True(LinguistModule.toKatakana(LinguistModule.fromHiragana("oranji じゅいす")) == "オランジ ジュイス");
        }

        [Fact]
        public void ToHiragana()
        {
            Assert.True(LinguistModule.toHiragana(LinguistModule.fromKatakana("oranji ジュイス")) == "おらんじ じゅいす");
        }

        [Fact]
        public void ToRomaji()
        {
            Assert.True(LinguistModule.fromHiragana(LinguistModule.fromKatakana("おらんじ ジュイス")) == "oranji juisu");
        }
    }
}
