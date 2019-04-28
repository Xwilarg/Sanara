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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV2.Games
{
    public abstract class AQuizz : AGame
    {
        public AQuizz(ITextChannel chan, List<string> dictionnary, Config config) : base(chan, dictionnary, config)
        { }

        protected override void Init()
        {
        }

        protected override bool CongratulateOnGuess()
            => true;

        protected override PostType GetPostType()
            => PostType.Url;

        protected override Task<string> GetCheckCorrectAsync(string userAnswer)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<string> GetLoose()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<string[]> GetPostAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
