using Discord;
using Sanara.Exception;

namespace Sanara.Game.MultiplayerMode
{
    public sealed class TurnByTurnMode : IMultiplayerMode
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
        }

        public string PrePost()
        {
            string mention;
            lock (_users)
            {
                mention = _users[_currentTurn].Mention;
            }
            return mention + " turns to play";
        }

        public void PreAnswerCheck(IUser user)
        {
            if (_currentTurn >= _users.Count || _users[_currentTurn].Id != user.Id)
                throw new InvalidGameAnswer("");
        }

        public void AnswerIsCorrect(IUser user)
        {
            // We assume that the user who answer is the current one (check done in PreAnswerCheck)
            lock (_users)
            {
                _currentTurn++;
                if (_currentTurn == _users.Count)
                    _currentTurn = 0;
            }
        }

        public bool Loose()
        {
            _users.RemoveAt(_currentTurn);
            if (_users.Count == 1) // If there is only one player remaining, he won
                return false;
            if (_currentTurn >= _users.Count)
                _currentTurn = 0;
            return true;
        }

        public string GetWinner()
            => _users[0].Mention;

        public bool CanLooseAuto()
            => true;

        public string GetOutroLoose()
            => null;

        public string GetRules()
            => "You must answer turn by turn, the last player standing win.";

        private List<IUser> _users;
        private int _currentTurn;
    }
}
