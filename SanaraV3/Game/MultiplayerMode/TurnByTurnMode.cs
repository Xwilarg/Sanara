using Discord;
using SanaraV3.Exception;
using System.Collections.Generic;

namespace SanaraV3.Game.MultiplayerMode
{
    public class TurnByTurnMode : IMultiplayerMode
    {
        public void Init(List<IUser> users)
        {
            var tmp = new List<IUser>(users);
            _users = new List<IUser>();
            while (tmp.Count > 0)
            {
                var index = StaticObjects.Random.Next(0, tmp.Count);
                _users.Add(tmp[index]);
                tmp.RemoveAt(index);
            }
            _currentTurn = 0;
            _remainingGames = 11;
        }

        public string PrePost()
        {
            _remainingGames--;
            if (_remainingGames == 0)
                throw new GameLost("Game ended");
            return _users[_currentTurn].Mention + " turns to play";
        }

        public void PreAnswerCheck(IUser user)
        {
            if (_users[_currentTurn] != user)
                throw new InvalidGameAnswer("");
        }

        public void AnswerIsCorrect(IUser user)
        {
            // We assume that the user who answer is the current one (check done in PreAnswerCheck)
            _currentTurn++;
            if (_currentTurn == _users.Count)
                _currentTurn = 0;
        }

        public bool Loose()
        {
            _users.RemoveAt(_currentTurn);
            if (_users.Count == 1) // If there is only one player remaining, he won
                return false;
            if (_currentTurn == _users.Count)
                _currentTurn = 0;
            return true;
        }

        public string GetWinner()
            => _users[0].Mention;

        public bool CanLooseAuto()
            => true;

        public string GetOutroLoose()
            => null;

        private List<IUser> _users;
        private int _currentTurn;
        private int _remainingGames;
    }
}
