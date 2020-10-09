using Discord;
using System.Collections.Generic;

namespace SanaraV3.Game
{
    public sealed class MultiplayerLobby
    {
        public MultiplayerLobby(IUser host)
        {
            _users = new List<IUser> { host };
        }

        public bool AddUser(IUser user)
        {
            if (_users.Contains(user))
                return false;
            _users.Add(user);
            return true;
        }

        public bool RemoveUser(IUser user)
        {
            if (!_users.Contains(user))
                return false;
            _users.Add(user);
            return true;
        }

        public int GetUserCount()
            => _users.Count;

        List<IUser> _users;
    }
}
