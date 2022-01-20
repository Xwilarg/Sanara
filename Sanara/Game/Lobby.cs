using Discord;
using Sanara.Game.MultiplayerMode;
using Sanara.Game.Preload;

namespace Sanara.Game
{
    public sealed class Lobby
    {
        public Lobby(IUser host, IPreload preload)
        {
            _users = new() { host };
            _lobbyOwner = host;
            _preload = preload;
            _multiType = MultiplayerType.VERSUS;
        }

        public MultiplayerType MultiplayerType
            => _users.Count <= 1 ? MultiplayerType.COOPERATION : _multiType;

        public bool ContainsUser(IUser user)
            => _users.Any(x => x.Id == user.Id);

        /// <returns>True is joined, false if leaved</returns>
        public bool ToggleUser(IUser user)
        {
            if (ContainsUser(user))
            {
                _users.RemoveAll(x => x.Id == user.Id);
                return false;
            }
            _users.Add(user);
            return true;
        }

        public void ToggleMultiplayerMode()
        {
            if (_multiType == MultiplayerType.COOPERATION)
            {
                _multiType = MultiplayerType.VERSUS;
            }
            else
            {
                _multiType = MultiplayerType.COOPERATION;
            }
        }

        public string[] GetAllMentions()
            => _users.Select(x => x.Mention).ToArray();

        public int GetUserCount()
            => _users.Count;

        public List<IUser> GetUsers()
            => _users;

        public bool IsHost(IUser user)
            => user.Id == _lobbyOwner.Id;

        public Embed GetIntroEmbed()
        {
            var embed = new EmbedBuilder
            {
                Description = string.Join("\n", _users.Select(u => u + (IsHost(u) ? " (Host)": "")))
            };
            embed.AddField("Rules", _preload.GetRules() + "\n\nIf the game break, you can use the \"/cancel\" command to force it to stop");
            embed.AddField($"Multiplayer Rules{(_users.Count > 1 ? "" : "(only if **more than 1 player** in the lobby)")}",
                _multiType == MultiplayerType.COOPERATION ?
                "All the player in the lobby can collaborate to find the answers" :
                "TODO");
            return embed.Build();
        }

        private readonly List<IUser> _users;
        private readonly IUser _lobbyOwner;
        private readonly IPreload _preload;
        private MultiplayerType _multiType;
    }
}
