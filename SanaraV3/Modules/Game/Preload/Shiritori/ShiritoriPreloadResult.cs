namespace SanaraV3.Modules.Game.Preload.Shiritori
{
    public struct ShiritoriPreloadResult : IPreloadResult
    {
        public ShiritoriPreloadResult(string word, string wordEnglish, string meanings)
        {
            Word = word;
            WordEnglish = wordEnglish;
            Meanings = meanings;
        }

        public string Word { get; } // Word in the local language
        public string WordEnglish { get; }  // Word in latin alphabet (ex: romaji for japanese)
        public string Meanings { get; }  // What the word mean
    }
}
