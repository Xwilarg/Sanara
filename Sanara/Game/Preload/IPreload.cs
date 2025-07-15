using Discord;
using Sanara.Compatibility;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload
{
    public interface IPreload
    {
        public void Init(IServiceProvider provider);
        /// <summary>
        /// Load the game dictionary
        /// </summary>
        public ReadOnlyCollection<IPreloadResult> Load();
        public string Name { get; }
        /// <summary>
        /// Create a new instance of a game and return it
        /// </summary>
        public AGame CreateGame(IMessageChannel msgchan, CommonUser user, GameSettings settings);
        /// <summary>
        /// Returns the game rules
        /// </summary>
        public string GetRules();
        /// <summary>
        /// Can the game be played in SFW channels
        /// </summary>
        public bool IsSafe();
    }
}
