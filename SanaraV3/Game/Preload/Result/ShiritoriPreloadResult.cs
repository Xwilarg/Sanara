using System.Collections.Generic;

namespace SanaraV3.Game.Preload.Result
{
    public sealed class ShiritoriPreloadResult : IPreloadResult
    {
        public ShiritoriPreloadResult(string word, string wordEnglish, string meanings)
        {
            Word = word;
            WordEnglish = wordEnglish;
            Meanings = meanings;
            LearningLevels = new List<int>();
        }

        public string Word { get; } // Word in the local language
        public string WordEnglish { get; }  // Word in latin alphabet (ex: romaji for japanese)
        public string Meanings { get; }  // What the word mean
        public List<int> LearningLevels { get; }
    }
}
