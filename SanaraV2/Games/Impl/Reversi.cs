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
            _board[3, 3] = 'X';
            _board[4, 4] = 'X';
            _board[3, 4] = 'O';
            _board[4, 3] = 'O';
            _player1 = true;
            _scorePlayer1 = 0;
            _scorePlayer2 = 0;
            _nbSkips = 0;
        }

        protected override bool CongratulateOnGuess()
            => false;

        protected override int? GetMaximumMultiplayer()
            => 2;

        protected override async Task NextTurnInternal()
        {
            if (!CanPlay())
            {
                await PostText(GetStringFromSentence(Sentences.ReversiCantPlay));
                _nbSkips++;
                if (_nbSkips == 2)
                    await EndOfGame();
                else
                {
                    _player1 = !_player1;
                    await ForceNextTurn();
                }
            }
        }

        protected override async Task<string> GetCheckCorrectAsync(string userAnswer)
        {
            string[] move = userAnswer.ToLower().Split(' ', ',', ';');
            if (move.Length == 1 && userAnswer.Length == 2)
                move = new string[] { userAnswer[0].ToString().ToLower(), userAnswer[1].ToString().ToLower() };
            if (move.Length != 2)
                return GetStringFromSentence(Sentences.ReversiInvalidMove);
            if (char.IsLetter(move[1][0]))
                move = new string[] { userAnswer[1].ToString().ToLower(), userAnswer[0].ToString().ToLower() };
            int case1, case2;
            if (move[0].Length != 1 || move[0][0] < 'a' || move[0][0] > 'h')
                return GetStringFromSentence(Sentences.ReversiInvalidMove);
            if (!int.TryParse(move[1], out case2) || case2 < 1 || case2 > 8)
                return GetStringFromSentence(Sentences.ReversiInvalidMove);
            case1 = move[0][0] - 'a';
            case2--;
            if (_board[case2, case1] != ' ')
                return GetStringFromSentence(Sentences.ReversiInvalidPos);
            bool moveValid = false;
            for (int i = -1; i <= 1; i++)
                for (int y = -1; y <= 1; y++)
                {
                    if (i == 0 && y == 0)
                        continue;
                    if (CheckLine(case2 + i, case1 + y, i, y, false))
                    {
                        moveValid = true;
                        CheckLine(case2 + i, case1 + y, i, y, true);
                    }
                }
            if (!moveValid)
                return GetStringFromSentence(Sentences.ReversiInvalidPos);
            _board[case2, case1] = _player1 ? 'X' : 'O';
            if (IsBoardFull())
            {
                await EndOfGame();
                return null;
            }
            _player1 = !_player1;
            _nbSkips = 0;
            return null;
        }

        private async Task EndOfGame()
        {
            CalculateScore();
            if ((_scorePlayer1 > _scorePlayer2 && !_player1)
                || (_scorePlayer1 < _scorePlayer2 && _player1))
                await ForceNextTurn();
            await LooseAsync(GetStringFromSentence(Sentences.ReversiGameEnded));
        }

        // There is probably a better way to do that
        private bool CanPlay()
        {
            for (int xPos = 0; xPos < 8; xPos++)
                for (int yPos = 0; yPos < 8; yPos++)
                    for (int i = -1; i <= 1; i++)
                        for (int y = -1; y <= 1; y++)
                        {
                            if (i == 0 && y == 0)
                                continue;
                            if (CheckLine(xPos + i, yPos + y, i, y, false))
                                return true;
                        }
            return false;
        }

        private void CalculateScore()
        {
            for (int i = 0; i < 8; i++)
                for (int y = 0; y < 8; y++)
                {
                    if (_board[i, y] == 'X')
                        _scorePlayer1++;
                    else if (_board[i, y] == 'O')
                        _scorePlayer2++;
                }
        }

        private bool IsBoardFull()
        {
            for (int i = 0; i < 8; i++)
                for (int y = 0; y < 8; y++)
                    if (_board[i, y] == ' ')
                        return false;
            return true;
        }

        private bool CheckLine(int x, int y, int xMove, int yMove, bool replace)
        {
            if (x < 0 || x >= 8 || y < 0 || y >= 8
                || _board[x, y] != (_player1 ? 'O' : 'X'))
                return false;
            while (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                if (_board[x, y] == (_player1 ? 'X' : 'O'))
                    return true;
                if (replace)
                    _board[x, y] = _player1 ? 'X' : 'O';
                x += xMove;
                y += yMove;
            }
            return false;
        }

        protected override async Task<string> GetLoose()
        {
            if (_scorePlayer1 == 0 && _scorePlayer2 == 0)
                CalculateScore();
            return GetStringFromSentence(Sentences.ReversiFinalScore) + Environment.NewLine +
                GetPlayerName(0) + ": " + _scorePlayer1 + Environment.NewLine +
                GetPlayerName(1) + ": " + _scorePlayer2;
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
        private int _nbSkips; // If 2 skips in a row, end of game

        // Used at the end of the game for scores
        private int _scorePlayer1, _scorePlayer2;
    }
}
