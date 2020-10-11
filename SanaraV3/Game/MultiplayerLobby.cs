using Discord;
using System.Collections.Generic;
using System.Linq;

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

        public string[] GetAllMentions()
            => _users.Select(x => x.Mention).ToArray();

        public int GetUserCount()
            => _users.Count;

        public List<IUser> GetUsers()
            => _users;

        List<IUser> _users;
    }
}
