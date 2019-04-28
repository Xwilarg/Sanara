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
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace SanaraV2.Games
{
    public abstract class AQuizz : AGame
    {
        public AQuizz(ITextChannel chan, List<string> dictionnary, Config config) : base(chan, dictionnary, config)
        { }

        protected override void Init()
        {
            _toGuess = null;
        }

        protected abstract Task<string[]> GetPostInternalAsync();

        protected override bool CongratulateOnGuess()
            => true;

        protected override PostType GetPostType()
            => PostType.Url;

        protected override async Task<string[]> GetPostAsync()
        {
            _toGuess = _dictionnary[Program.p.rand.Next(_dictionnary.Count)];
            return await GetPostInternalAsync();
        }

        protected override async Task<string> GetCheckCorrectAsync(string userAnswer)
        {
            string cleanUserAnswer = Utilities.CleanWord(userAnswer);
            string cleanToGuess = Utilities.CleanWord(_toGuess);
            if (cleanUserAnswer == cleanToGuess)
                return null;
            if (cleanUserAnswer.Contains(cleanToGuess) || cleanToGuess.Contains(cleanUserAnswer))
                return Sentences.BooruGuessClose(GetGuildId(), userAnswer);
            return (Sentences.GuessBad(GetGuildId(), userAnswer));
        }

        protected override async Task<string> GetLoose()
            => Sentences.GoodAnswerWas(GetGuildId(), FormatAnswer());

        private string FormatAnswer()
            => new CultureInfo("en-US", false).TextInfo.ToTitleCase(_toGuess.Replace('_', ' '));

        protected string _toGuess; // Word the player have to guess
    }
}
