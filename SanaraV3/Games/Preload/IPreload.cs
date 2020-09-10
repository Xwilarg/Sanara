using Discord;
using System.Collections.ObjectModel;

namespace SanaraV3.Games.Preload
{
    public interface IPreload
    {
        public ReadOnlyCollection<IPreloadResult> Load(); // Load the game dictionary
        public string[] GetGameNames(); // Get the names used to launch the game
        public string GetNameArg(); // If need a special argument to be played, let to null if none
        public AGame CreateGame(IMessageChannel msgchan, IUser user, GameSettings settings); // Create a new instance of a game and return it
    }
}
