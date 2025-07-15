using Discord;
using Sanara.Compatibility;
using Sanara.Game.Preload;

namespace Sanara.Game
{
    public class ReplayLobby
    {
        public ReplayLobby(IPreload preload, CommonUser lastHost, List<CommonUser> users, MultiplayerType versusType)
        {
            Preload = preload;
            LastHost = lastHost;
            _users = users;
            _ready = new();
            _versusType = versusType;
            _creationTime = DateTime.Now;
        }

        public CommonEmbedBuilder GetEmbed()
        {
            return new CommonEmbedBuilder
            {
                Title = "Replay?",
                Description = string.Join("\n", _users.Select(x => x.ToString() + ": " + (_ready.Any(r => r.Id == x.Id) ? "Ready" : "**Not ready**"))),
                Color = Color.Orange
            };
        }

        public bool ToggleReady(CommonUser user)
        {
            if (_users.Any(x => x.Id == user.Id))
            {
                if (_ready.Any(x => x.Id == user.Id))
                {
                    _ready.RemoveAll(x => x.Id == user.Id);
                }
                else
                {
                    _ready.Add(user);
                }
                return true;
            }
            return false;
        }

        public bool IsAllReady => _ready.Count == _users.Count;

        public bool HasExpired => DateTime.Now.Subtract(_creationTime).TotalHours > 2;

        public Lobby CreateLobby()
        {
            var lobby = new Lobby(LastHost, Preload);
            lobby.SetMultiplayerMode(_versusType);
            foreach (var user in _users)
            {
                if (user.Id != LastHost.Id) // Was already added
                {
                    lobby.AddUser(user);
                }
            }
            return lobby;
        }

        public IUserMessage Message { set; get; }

        public IPreload Preload { private set; get; }
        private List<CommonUser> _users, _ready;
        public CommonUser LastHost { private set; get; }
        private DateTime _creationTime;
        private MultiplayerType _versusType;
    }
}
