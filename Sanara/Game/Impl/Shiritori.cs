using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Exception;
using Sanara.Game.MultiplayerMode;
using Sanara.Game.Preload;
using Sanara.Game.Preload.Result;
using Sanara.Module.Utility;
using System.Web;

namespace Sanara.Game.Impl
{
    public sealed class Shiritori : AGame
    {
        public Shiritori(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings, int minWordLength) : base(textChan, user, preload, StaticObjects.ModeText, new TurnByTurnMode(), settings)
        {
            _words = new List<ShiritoriPreloadResult>(preload.Load().Cast<ShiritoriPreloadResult>());
            _isFirst = true;
            _alreadySaid = new List<string>();
            _lastUserChoice = null;
            _minWordLength = minWordLength;
        }

        public Shiritori(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, preload, StaticObjects.ModeText, new TurnByTurnMode(), settings)
        {
            _words = new List<ShiritoriPreloadResult>(preload.Load().Cast<ShiritoriPreloadResult>());
            _isFirst = true;
            _alreadySaid = new List<string>();
            _lastUserChoice = null;
            _minWordLength = 2;
        }

        protected override string[] GetPostInternal()
        {
            if (_lobby.MultiplayerType == MultiplayerType.VERSUS)
            {
                if (_lastUserChoice == null)
                    return Array.Empty<string>();
                else
                    return new[] { _lastUserChoice };
            }
            else
            {
                if (_isFirst)
                {
                    _isFirst = false;
                    _alreadySaid.Add("しりとり");
                    _currWord = "しりとり";
                    return new[] { "しりとり (shiritori)" };
                }
                var randomWord = GetRandomValidWord(GetWordEnding(_currWord));
                if (randomWord == null)
                    throw new GameLost("I don't know any work beginning by " + GetWordEnding(_currWord));
                _words.Remove(randomWord);
                _alreadySaid.Add(randomWord.Word);
                _currWord = randomWord.Word;
                return new[] { randomWord.Word + $" ({randomWord.WordEnglish} - Meaning: {randomWord.Meanings})" };
            }
        }

        protected override async Task CheckAnswerInternalAsync(Module.Command.ICommandContext answer)
        {
            // We convert to hiragana so it's then easier to check if the word really exist
            // Especially for some edge case, like りゅう (ryuu) is starting by "ri" and not by "ry"
            string hiraganaAnswer = Language.ToHiragana(answer.GetArgument<string>("answer").ToLowerInvariant());

            if (hiraganaAnswer.Any(c => c < 0x0041 || (c > 0x005A && c < 0x0061) || (c > 0x007A && c < 0x3041) || (c > 0x3096 && c < 0x30A1) || c > 0x30FA))
                throw new InvalidGameAnswer("Your answer must be in hiragana, katakana or romaji");

            if (_lobby.MultiplayerType == MultiplayerType.VERSUS && _lastUserChoice == null)
            {
                if (hiraganaAnswer != "しりとり")
                    throw new InvalidGameAnswer("Your first word must be しりとり (shiritori)");
                _alreadySaid.Add("しりとり");
                _currWord = "しりとり";
                _lastUserChoice = "しりとり (shiritori)";
                return;
            }

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
                    reading = Language.ToHiragana(readingObj.Value<string>());
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
                throw new InvalidGameAnswer($"Your word must begin by {ending} ({Language.ToRomaji(ending)}).");
            if (_alreadySaid.Contains(hiraganaAnswer))
                throw new GameLost("This word was already said.");
            if (!isNoun)
                throw new InvalidGameAnswer("Your word must be a noun.");
            if (!IsLongEnough(hiraganaAnswer, _minWordLength)) // We can't just check the word count since しゃ would count as only one character
                throw new InvalidGameAnswer($"Your word must be at least {_minWordLength} syllable.");
            if (hiraganaAnswer.Last() == 'ん')
                throw new GameLost("Your word is finishing with a ん (n).");
            if (_words.Any(x => x.Word == hiraganaAnswer))
                _words.Remove(_words.Where(x => x.Word == hiraganaAnswer).First());
            _alreadySaid.Add(hiraganaAnswer);
            _currWord = hiraganaAnswer;
            _lastUserChoice = _currWord + $" ({Language.ToRomaji(_currWord)}) - Meaning: {string.Join(", ", meanings)}";
        }

        protected override string GetAnswer()
        {
            if (_lobby.MultiplayerType == MultiplayerType.VERSUS && _lastUserChoice == null)
                return "Your first word must be しりとり (shiritori)";
            ShiritoriPreloadResult word = null;
            var ending = GetWordEnding(_currWord);
            for (int i = 5; i >= 1; i--)
            {
                var validWords = _words.Where(x => x.Word.StartsWith(ending) && x.LearningLevels.Contains(i)).ToArray();
                if (validWords.Length == 0)
                    continue;
                word = validWords[StaticObjects.Random.Next(validWords.Length)];
                break;
            }
            if (word == null)
                word = GetRandomValidWord(ending);
            if (word == null)
                return null;
            return $"Here's a word you could have said: {word.Word} ({word.WordEnglish}) - Meaning: {word.Meanings}";
        }

        protected override string GetSuccessMessage(IUser _)
            => null;

        private ShiritoriPreloadResult GetRandomValidWord(string ending)
        {
            var validWords = _words.Where(x => x.Word.StartsWith(ending)).ToArray(); // Valid words are the ones beginning by the ending of the current word
            if (validWords.Length == 0)
                return null;
            return validWords[StaticObjects.Random.Next(validWords.Length)];
        }

        public static bool IsLongEnough(string word, int requiredLength)
        {
            while (requiredLength > 0)
            {
                if (word.Length == 0)
                    return false;
                var ending = GetWordEnding(word);
                word = word.Substring(0, word.Length - ending.Length);
                requiredLength--;
            }
            return true;
        }

        /// <summary>
        /// We get the latest character (cause in shiritori the next word must begin by the ending of the last one)
        /// But in a word like じてんしゃ we need to get the "しゃ" and not the "ゃ"
        /// </summary>
        private static string GetWordEnding(string word) // TODO: Doesn't handle things like っし
        {
            char lastChar = word.Last();
            if (lastChar == 'ゃ' || lastChar == 'ぃ' || lastChar == 'ゅ'
                || lastChar == 'ぇ' || lastChar == 'ょ')
                return word.Substring(word.Length - 2, 2);
            return lastChar.ToString();
        }

        protected override int GetGameTime()
            => 15;

        private readonly List<ShiritoriPreloadResult> _words;
        private bool _isFirst; // Is the first word (because we must start by saying "shiritori")
        private readonly List<string> _alreadySaid; // A word can't be said twice
        private string _currWord; // The last word that was said
        private int _minWordLength;

        // Used for multiplayer
        private string? _lastUserChoice;
    }
}
