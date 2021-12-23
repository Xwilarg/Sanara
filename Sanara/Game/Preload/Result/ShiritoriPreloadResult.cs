namespace Sanara.Game.Preload.Result
{
    public sealed class ShiritoriPreloadResult : IPreloadResult
    {
        public ShiritoriPreloadResult(string word, string wordEnglish, string meanings)
        {
            Word = word;
            WordEnglish = wordEnglish;
            Meanings = meanings;
            LearningLevels = new();
        }

        /// <summary>
        /// Word in the local language
        /// </summary>
        public string Word { get; }
        /// <summary>
        /// Word in latin alphabet (ex: romaji for japanese)
        /// </summary>
        public string WordEnglish { get; }
        /// <summary>
        /// What the word mean
        /// </summary>
        public string Meanings { get; }
        public List<int> LearningLevels { get; }
    }
}
