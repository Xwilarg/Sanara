using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exceptions;
using SanaraV3.Modules.Game.Preload;
using SanaraV3.Modules.Game.Preload.Shiritori;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV3.Modules.Game.Impl
{
    public sealed class Shiritori : AGame
    {
        public Shiritori(IMessageChannel textChan, IPreload preload) : base(textChan, StaticObjects.ModeText)
        {
            _words = preload.Load().Cast<ShiritoriPreloadResult>().ToList();
            _isFirst = true;
            _alreadySaid = new List<string>();
        }

        protected override string GetPostInternal()
        {
            if (_isFirst)
            {
                _isFirst = false;
                _alreadySaid.Add("しりとり");
                _currWord = "しりとり";
                return "しりとり (shiritori)";
            }
            var randomWord = GetRandomValidWord(GetWordEnding(_currWord));  
            _words.Remove(randomWord);
            _alreadySaid.Add(randomWord.Word);
            _currWord = randomWord.Word;
            return randomWord.Word + $" ({randomWord.WordEnglish} - Meaning: {randomWord.Meanings})";
        }

        protected override async Task CheckAnswerInternalAsync(string answer)
        {
            // We convert to hiragana so it's then easier to check if the word really exist
            // Especially for some edge case, like りゅう (ryuu) is starting by "ri" and not by "ry"
            string hiraganaAnswer = Tool.LanguageModule.ToHiragana(answer);

            if (hiraganaAnswer.Any(c => c < 0x0041 || (c > 0x005A && c < 0x0061) || (c > 0x007A && c < 0x3041) || (c > 0x3096 && c < 0x30A1) || c > 0x30FA))
                throw new InvalidGameAnswer("Your answer must be in hiragana, katakana or romaji");

            JObject json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("http://jisho.org/api/v1/search/words?keyword=" + HttpUtility.UrlEncode(string.Join("%20", hiraganaAnswer))));
            var data = (JArray)json["data"];
            if (data.Count == 0)
                throw new InvalidGameAnswer("This word doesn't exist.");

            bool isCorrect = false, isNoun = false;
            string reading;
            string[] meanings = new string[0];
            // For each answer we check if it match our answer
            foreach (var d in data)
            {
                foreach (var jp in d["japanese"])
                {
                    var readingObj = jp["reading"];
                    if (readingObj == null)
                        continue;
                    reading = Tool.LanguageModule.ToHiragana(readingObj.Value<string>());
                    if (reading == hiraganaAnswer)
                    {
                        isCorrect = true;
                        // For each meaning we check if the word is a noun
                        foreach (var meaning in d["senses"])
                        {
                            foreach (var partSpeech in meaning["parts_of_speech"])
                            {
                                if (partSpeech.Value<string>() == "Noun")
                                {
                                    isNoun = true;
                                    meanings = ((JArray)meaning["english_definitions"]).Select(x => (string)x).ToArray();
                                    goto ContinueCheck; // The word is valid, we are done with the basic checks
                                }
                            }
                        }
                    }
                }
            }
            ContinueCheck:
            if (!isCorrect)
                throw new InvalidGameAnswer("This word doesn't exist.");
            var ending = GetWordEnding(_currWord);
            if (!hiraganaAnswer.StartsWith(ending))
                throw new InvalidGameAnswer($"Your word must begin by {ending} ({Tool.LanguageModule.ToRomaji(ending)}).");
            if (!isNoun)
                throw new InvalidGameAnswer("Your word must be a noun.");
            if (hiraganaAnswer == GetWordEnding(hiraganaAnswer)) // We can't just check the word count since しゃ would count as only one character
                throw new InvalidGameAnswer("Your word must be at least 2 characters.");
            if (_alreadySaid.Contains(hiraganaAnswer))
                throw new GameLost("This word was already said.");
            if (hiraganaAnswer.Last() == 'ん')
                throw new GameLost("Your word is finishing with a ん (n).");
            if (_words.Any(x => x.Word == hiraganaAnswer))
                _words.Remove(_words.Where(x => x.Word == hiraganaAnswer).First());
            _alreadySaid.Add(hiraganaAnswer);
            _currWord = hiraganaAnswer;
        }

        protected override string GetAnswer()
        {
            var word = GetRandomValidWord(GetWordEnding(_currWord));
            return $"Here's a word you could have said: {word.Word} ({word.WordEnglish}) - Meaning: {word.Meanings}";
        }

        private ShiritoriPreloadResult GetRandomValidWord(string ending)
        {
            var validWords = _words.Where(x => x.Word.StartsWith(ending)).ToArray(); // Valid words are the ones beginning by the ending of the current word
            return validWords[StaticObjects.Random.Next(validWords.Length)];
        }

        /// <summary>
        /// We get the latest character (cause in shiritori the next word must begin by the ending of the last one)
        /// But in a word like じてんしゃ we need to get the "しゃ" and not the "ゃ"
        /// </summary>
        private string GetWordEnding(string word)
        {
            char lastChar = word.Last();
            if (lastChar == 'ゃ' || lastChar == 'ぃ' || lastChar == 'ゅ'
                || lastChar == 'ぇ' || lastChar == 'ょ')
                return word.Substring(word.Length - 2, 2);
            return lastChar.ToString();
        }

        protected override int GetGameTime()
            => 15;

        private List<ShiritoriPreloadResult> _words;
        private bool _isFirst; // Is the first word (because we must start by saying "shiritori")
        private List<string> _alreadySaid; // A word can't be said twice
        private string _currWord; // The last word that was said
    }
}
