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
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class ReversiPreload : APreload
    {
        public ReversiPreload() : base(new[] { "reversi" }, 15, null)
        { }

        public override bool IsNsfw()
            => false;

        public override bool DoesAllowFull()
            => false;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.MultiOnly;

        public override string GetRules(ulong guildId, bool _)
            => null;
    }

    public class Reversi : AGame
    {
        public Reversi(ITextChannel chan, Config config, ulong playerId) : base(chan, null, config, playerId)
        { }

        protected override bool CongratulateOnGuess()
            => false;

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

        protected override PostType GetPostType()
            => PostType.Text;

        protected override string Help()
        {
            throw new System.NotImplementedException();
        }

        protected override void Init()
        { }
    }
}
