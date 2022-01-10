using Sanara.Game.Preload.Result;
using Sanara.Module.Utility;

namespace Sanara.Game.Preload.Impl.Static
{
    public static class Shiritori
    {
        public static List<ShiritoriPreloadResult> GetWords()
            => _words;

        public static string GetRules()
            => "Shiritori is a Japanese game where you must find a word starting by the last syllable of the previous one.\n" +
            "For example if someone say りゅう (ryuu, dragon) you have to say a word starting by う (u), like うさぎ (usagi, rabbit).";

        static Shiritori()
        {
            if (!File.Exists("Saves/Game/ShiritoriJapanese.txt"))
                File.WriteAllBytes("Saves/Game/ShiritoriJapanese.txt", StaticObjects.HttpClient.GetByteArrayAsync("https://files.zirk.eu/Sanara/ShiritoriJapanese.txt").GetAwaiter().GetResult());
            string[] lines = File.ReadAllLines("Saves/Game/ShiritoriJapanese.txt");
            _words = new();
            foreach (var l in lines)
            {
                string[] curr = l.Split('$');
                string word = curr[0];
                _words.Add(new ShiritoriPreloadResult(word, Language.ToRomaji(word), curr[1]));
            }
            for (int i = 5; i >= 1; i--)
            {
                if (!File.Exists($"Saves/Game/Jlpt{i}Vocabulary.txt"))
                    File.WriteAllBytes($"Saves/Game/Jlpt{i}Vocabulary.txt", StaticObjects.HttpClient.GetByteArrayAsync("https://files.zirk.eu/Sanara/Jlpt" + i + "Vocabulary.txt").GetAwaiter().GetResult());
                string[] jlptLines = File.ReadAllLines($"Saves/Game/Jlpt{i}Vocabulary.txt");
                foreach (var l in jlptLines)
                {
                    string[] curr = l.Split('$');
                    string word = curr[0];
                    var value = _words.Find(x => x.Word == word);
                    if (value == null)
                    {
                        value = new ShiritoriPreloadResult(word, Language.ToRomaji(word), curr[1]);
                        _words.Add(value);
                    }
                    value.LearningLevels.Add(i);
                }
            }
            _words = _words.Where(x => !x.Word.EndsWith("ん") && x.Word.Length > 1).ToList();
        }

        private static List<ShiritoriPreloadResult> _words;
    }
}
