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
using System;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class ReversiPreload : APreload
    {
        public ReversiPreload() : base(new[] { "reversi" }, 60, Sentences.ReversiGame)
        { }

        public override bool IsNsfw()
            => false;

        public override bool DoesAllowFull()
            => false;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.MultiOnly;

        public override string GetRules(ulong guildId, bool _)
            => Sentences.RulesReversi(guildId);
    }

    public class Reversi : AGame
    {
        public Reversi(ITextChannel chan, Config config, ulong playerId) : base(chan, null, config, playerId, true)
        { }

        protected override void Init()
        {
            _board = new char[8, 8];
            for (int i = 0; i < 8; i++)
                for (int y = 0; y < 8; y++)
                    _board[i, y] = ' ';
            _player1 = true;
        }

        protected override bool CongratulateOnGuess()
            => false;

        protected override int? GetMaximumMultiplayer()
            => 2;

        protected override async Task<string> GetCheckCorrectAsync(string userAnswer)
        {
            string[] move = userAnswer.ToLower().Split(' ', ',', ';');
            if (move.Length == 1 && userAnswer.Length == 2)
                move = new string[] { userAnswer[0].ToString().ToLower(), userAnswer[1].ToString().ToLower() };
            if (move.Length != 2)
                return "Invalid move";
            int case1, case2;
            if (move[0].Length != 1 || move[0][0] < 'a' || move[0][0] > 'h')
                return "Must be between A and H";
            if (!int.TryParse(move[1], out case2) || case2 < 1 || case2 > 8)
                return "Must be between 1 and 8";
            case1 = move[0][0] - 'a';
            case2--;
            if (_board[case2, case1] != ' ')
                return "Can't play here";
            _board[case2, case1] = _player1 ? 'X' : 'O';
            _player1 = !_player1;
            return null;
        }

        protected override async Task<string> GetLoose()
        {
            throw new NotImplementedException();
        }

        protected override async Task<string[]> GetPostAsync()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("```");
            str.AppendLine("  A B C D E F G H");
            str.AppendLine(" ┌─┬─┬─┬─┬─┬─┬─┬─┐");
            for (int i = 0; i < 8; i++)
            {
                str.Append((i + 1).ToString() + "│");
                for (int y = 0; y < 8; y++)
                {
                    str.Append(_board[i, y]);
                    str.Append("│");
                }
                str.AppendLine();
                if (i != 7)
                    str.AppendLine(" ├─┼─┼─┼─┼─┼─┼─┼─┤");
            }
            str.AppendLine(" └─┴─┴─┴─┴─┴─┴─┴─┘");
            str.AppendLine("```");
            return new[] { str.ToString() };
        }

        protected override PostType GetPostType()
            => PostType.Text;

        protected override string Help()
            => null;

        private char[,] _board; // Game board
        private bool _player1; // Is it player 1 or player 2 turn
    }
}
