using Discord;

namespace Sanara.Game
{
    public sealed class Lobby
    {
        public Lobby(IUser host)
        {
            _users = new() { host };
            _lobbyOwner = host;
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

        public bool IsHost(IUser user)
            => user == _lobbyOwner;

        private readonly List<IUser> _users;
        private readonly IUser _lobbyOwner;
    }
}
