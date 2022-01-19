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
            if (_users.Any(x => x.Id == user.Id))
                return false;
            _users.Add(user);
            return true;
        }

        public bool ContainsUser(IUser user)
            => _users.Any(x => x.Id == user.Id);

        public string[] GetAllMentions()
            => _users.Select(x => x.Mention).ToArray();

        public int GetUserCount()
            => _users.Count;

        public List<IUser> GetUsers()
            => _users;

        public bool IsHost(IUser user)
            => user == _lobbyOwner;

        public bool IsMultiplayer => GetUserCount() > 1;

        private readonly List<IUser> _users;
        private readonly IUser _lobbyOwner;
    }
}
