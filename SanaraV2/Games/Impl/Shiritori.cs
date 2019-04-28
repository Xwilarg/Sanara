/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.

using Discord;
using Newtonsoft.Json;
using SanaraV2.Features.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class ShiritoriPreload : APreload
    {
        public ShiritoriPreload() : base(LoadDictionnary(), new[] { "shiritori" }, 15, Sentences.ShiritoriGame)
        { }

        public override bool IsNsfw()
            => false;

        public override bool DoesAllowFull()
            => false;

        public override string GetRules(ulong guildId)
            => Sentences.RulesShiritori(guildId) + Environment.NewLine + Sentences.RulesShiritori2(guildId);

        private static List<string> LoadDictionnary()
        {
            if (!File.Exists("Saves/shiritoriWords.dat"))
                return (null);
            return (File.ReadAllLines("Saves/shiritoriWords.dat").ToList());
        }
    }

    public class Shiritori : AGame
    {
        public Shiritori(ITextChannel chan, List<string> dictionnary, Config config) : base(chan, dictionnary, config)
        { }

        protected override void Init()
        {
            _alreadySaid = new List<string>();
            _currWord = null;
        }

        protected override async Task<string[]> GetPostAsync()
        {
            if (_currWord == null) // The bot start the game by saying しりとり
            {
                _currWord = "しりとり";
                _dictionnary.Remove(_dictionnary.Find(x => x.Split('$')[0] == _currWord));
                _alreadySaid.Add("しりとり");
                return (new string[] { "しりとり (shiritori)" });
            }
            string[] validWords = GetValidWords();
            if (validWords.Length == 0) // Not supposed to happen
            {
                LooseException le = new LooseException(GetStringFromSentence(Sentences.ShiritoriNoWord));
                await Program.p.LogError(new LogMessage(LogSeverity.Error, le.Source, le.Message, le));
                throw le;
            }
            string word = validWords[Program.p.rand.Next(0, validWords.Length)];
            string[] splitWord = word.Split('$');
            _dictionnary.Remove(word);
            _alreadySaid.Add(splitWord[0]);
            _currWord = Linguist.ToHiragana(splitWord[0]);
            return (new string[] { splitWord[0] + " (" + Linguist.ToRomaji(splitWord[0]) + ") - " + GetStringFromSentence(Sentences.Meaning) + ": " + splitWord[1] });
        }

        protected override PostType GetPostType()
            => PostType.Text;

        protected override bool CongratulateOnGuess()
            => false;

        protected override async Task<string> GetCheckCorrectAsync(string userAnswer)
        {
            userAnswer = Linguist.ToHiragana(userAnswer);
            if (userAnswer.Any(c => c < 0x0041 || (c > 0x005A && c < 0x0061) || (c > 0x007A && c < 0x3041) || (c > 0x3096 && c < 0x30A1) || c > 0x30FA))
                return GetStringFromSentence(Sentences.OnlyHiraganaKatakanaRomaji);
            dynamic json;
            using (HttpClient hc = new HttpClient())
                json = JsonConvert.DeserializeObject(await hc.GetStringAsync("http://www.jisho.org/api/v1/search/words?keyword=" + Uri.EscapeDataString(userAnswer)));
            if (json.data.Count == 0)
                return GetStringFromSentence(Sentences.ShiritoriDoesntExist);
            bool isCorrect = false, isNoun = false;
            foreach (dynamic s in json.data)
            {
                foreach (dynamic jp in s.japanese)
                {
                    if (Linguist.ToHiragana((string)jp.reading) == userAnswer)
                    {
                        isCorrect = true;
                        foreach (dynamic meaning in s.senses)
                        {
                            foreach (dynamic partSpeech in meaning.parts_of_speech)
                            {
                                if (partSpeech == "Noun")
                                {
                                    isNoun = true;
                                    goto ContinueCheck;
                                }
                            }
                        }
                    }
                }
            }
            ContinueCheck:
            if (!isCorrect)
                return GetStringFromSentence(Sentences.ShiritoriDoesntExist);
            string lastCharac = GetLastCharacter(_currWord);
            if (!userAnswer.StartsWith(GetLastCharacter(_currWord)))
                return Sentences.ShiritoriMustBegin(GetGuildId(), lastCharac, Linguist.ToRomaji(lastCharac));
            if (!isNoun)
                return GetStringFromSentence(Sentences.ShiritoriNotNoun);
            if (userAnswer.Length == 1)
                return GetStringFromSentence(Sentences.ShiritoriTooSmall);
            if (_alreadySaid.Contains(userAnswer))
            {
                await LooseAsync(GetStringFromSentence(Sentences.ShiritoriAlreadySaid));
                return "";
            }
            if (userAnswer.Last() == 'ん')
            {
                await LooseAsync(GetStringFromSentence(Sentences.ShiritoriEndWithN));
                return "";
            }
            _dictionnary.Remove(_dictionnary.Find(x => x.Split('$')[0] == userAnswer));
            _alreadySaid.Add(userAnswer);
            _currWord = userAnswer;
            return null;
        }

        protected override async Task<string> GetLoose()
        {
            string[] validWords = GetValidWords();
            if (validWords.Length == 0)
                return GetStringFromSentence(Sentences.ShiritoriNoMoreWord);
            string word = validWords[Program.p.rand.Next(0, validWords.Length)];
            string[] splitWord = word.Split('$');
            return Sentences.ShiritoriSuggestion(GetGuildId(), splitWord[0], Linguist.ToRomaji(splitWord[0]), splitWord[1]);
        }

        private string[] GetValidWords()
            => _dictionnary.Where(x => x.StartsWith(GetLastCharacter(_currWord))).ToArray(); // Sanara word must begin by the ending of the player word

        private string GetLastCharacter(string word)
        {
            char lastChar = word.Last();
            if (lastChar == 'ゃ' || lastChar == 'ぃ' || lastChar == 'ゅ'
                || lastChar == 'ぇ' || lastChar == 'ょ')
                return (word.Substring(word.Length - 2, 2));
            return (lastChar.ToString());
        }

        private List<string>    _alreadySaid; // We make sure that the user don't say the same word twice
        private string          _currWord; // The current word
    }
}
