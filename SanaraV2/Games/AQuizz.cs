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
using SanaraV2.Modules.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV2.Games
{
    public abstract class AQuizz : AGame
    {
        protected AQuizz(ITextChannel chan, List<string> dictionnary, Config config, ulong playerId) : base(chan, dictionnary, config, playerId)
        { }

        protected override void Init()
        {
            _toGuess = null;
        }

        protected abstract Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr);
        protected abstract bool IsDictionnaryFull(); // Does dictionnary contains all possibility ?
        protected abstract bool DoesDisplayHelp();

        protected override bool CongratulateOnGuess()
            => true;

        protected override PostType GetPostType()
            => PostType.Url;

        protected override async Task<string[]> GetPostAsync()
        {
            if (_dictionnary.Count == 0)
                throw new LooseException(GetStringFromSentence(Sentences.DictionnaryEmpty));
            int index = Program.p.rand.Next(_dictionnary.Count);
            var elem = await GetPostInternalAsync(_dictionnary[index]);
            _toGuess = elem.Item2;
            _dictionnary.RemoveAt(index);
            return elem.Item1;
        }

        /// Only for unit tests
        public async Task<string> GetUrlTest(string userAnswer)
            => (await GetPostInternalAsync(userAnswer)).Item1[0];

        protected override async Task<string> GetCheckCorrectAsync(string userAnswer)
        {
            string cleanUserAnswer = Utilities.CleanWord(userAnswer);
            foreach (string s in _toGuess)
            {
                if (cleanUserAnswer == Utilities.CleanWord(s))
                    return null;
            }
            if (IsDictionnaryFull())
            {
                if (!_dictionnary.Any(x => Utilities.CleanWord(x) == cleanUserAnswer))
                    return "❌";
            }
            foreach (string s in _toGuess)
            {
                string cleanGuess = Utilities.CleanWord(s);
                if (cleanUserAnswer.Contains(cleanGuess) || cleanGuess.Contains(cleanUserAnswer))
                    return Sentences.BooruGuessClose(GetGuildId(), userAnswer.Replace("@everyone", "@ everyone"));
            }
            return "❌";
        }

        protected override async Task<string> GetLoose()
            => Sentences.GoodAnswerWas(GetGuildId(), FormatAnswer());

        private string FormatAnswer()
            => HttpUtility.UrlDecode(new CultureInfo("en-US", false).TextInfo.ToTitleCase(_toGuess[0].Replace('_', ' ')));

        protected override string Help()
        {
            if (!DoesDisplayHelp())
                return null;
            string help = _toGuess[0].First().ToString().ToUpper();
            foreach (char c in _toGuess[0].Skip(1))
            {
                if (c == '_')
                    help += ' ';
                else
                    help += "\\*";
            }
            return help;
        }

        protected string[] _toGuess; // Word the player have to guess
    }
}
