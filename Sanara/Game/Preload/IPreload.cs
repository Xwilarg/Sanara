using Discord;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload
{
    public interface IPreload
    {
        public void Init();
        /// <summary>
        /// Load the game dictionary
        /// </summary>
        public ReadOnlyCollection<IPreloadResult> Load();
        /// <summary>
        /// Get the names used to launch the game
        /// </summary>
        public string[] GetGameNames();
        /// <summary>
        /// If need a special argument to be played
        /// </summary>
        public string GetNameArg();
        /// <summary>
        /// Create a new instance of a game and return it
        /// </summary>
        public AGame CreateGame(IMessageChannel msgchan, IUser user, GameSettings settings);
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
