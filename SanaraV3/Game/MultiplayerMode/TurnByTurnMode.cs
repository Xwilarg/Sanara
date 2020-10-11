using Discord;
using SanaraV3.Exception;
using System.Collections.Generic;

namespace SanaraV3.Game.MultiplayerMode
{
    public class TurnByTurnMode : IMultiplayerMode
    {
        public void Init(List<IUser> users)
        {
            while (users.Count > 0)
            {
                var index = StaticObjects.Random.Next(0, users.Count);
                _users.Add(users[index]);
                users.RemoveAt(index);
            }
            _currentTurn = 0;
        }

        public string PrePost()
            => _users[_currentTurn].Mention + " turns to play";

        public void PreAnswerCheck(IUser user)
        {
            if (_users[_currentTurn] != user)
                throw new InvalidGameAnswer("");
        }

        private List<IUser> _users;
        private int _currentTurn;
    }
}
