using Discord;
using Sanara.Compatibility;
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
            _creationTime = DateTime.Now;
        }

        public string InitVersusRules(string rules)
            => _versusRules = rules;

        public MultiplayerType MultiplayerType
            => _users.Count <= 1 ? MultiplayerType.COOPERATION : _multiType;

        public bool ContainsUser(IUser user)
            => _users.Any(x => x.Id == user.Id);

        public void AddUser(IUser user)
        {
            if (!ContainsUser(user))
            {
                _users.Add(user);
            }
        }

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

        public void SetMultiplayerMode(MultiplayerType type) => _multiType = type;
        public MultiplayerType GetMultiplayerMode() => _multiType;

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

        public IUser Host => _lobbyOwner;

        public CommonEmbedBuilder GetIntroEmbed()
        {
            string multiRules;
            if (_users.Count > 1)
            {
                if (_multiType == MultiplayerType.COOPERATION)
                {
                    multiRules = "All the player in the lobby can collaborate to find the answers";
                }
                else
                {
                    multiRules = _versusRules;
                }
            }
            else
            {
                multiRules = "Only applicable if lobby has more than 1 player";
            }

            var embed = new CommonEmbedBuilder
            {
                Description = string.Join("\n", _users.Select(u => $"**{u}**" + (IsHost(u) ? " (Host)": "")))
            };
            embed.AddField("Rules", _preload.GetRules() + "\n\nIf the game break, you can use the \"/cancel\" command to force it to stop");
            embed.AddField($"Multiplayer Rules", multiRules);
            return embed;
        }

        public bool HasExpired => DateTime.Now.Subtract(_creationTime).TotalHours > 2;

        /// <summary>
        /// List of users in the lobby
        /// </summary>
        private readonly List<IUser> _users;
        /// <summary>
        /// User that created the lobby
        /// </summary>
        private readonly IUser _lobbyOwner;
        /// <summary>
        /// Game preload information
        /// </summary>
        private readonly IPreload _preload;
        /// <summary>
        /// Is the game cooperation or versus
        /// </summary>
        private MultiplayerType _multiType;
        /// <summary>
        /// If we are versus, the current rules
        /// </summary>
        private string _versusRules;
        private DateTime _creationTime;
    }
}
