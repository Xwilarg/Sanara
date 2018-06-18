using Xunit;
using SanaraV2;
using VndbSharp;

namespace Sanara_UnitTests
{
    public class Program
    {
        [Fact]
        public void ToKatakana()
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

        [Fact]
        public async void Vn()
        {
            Assert.True((await VndbModule.getVn("hoshizora no memoria wish upon a shooting star")).Id == 1474);
        }
    }
}
