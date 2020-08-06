using Discord;

namespace SanaraV3.Modules.Game.Preload
{
    public interface IPreload
    {
        public IPreloadResult[] Load(); // Load the game dictionary
        public string[] GetGameNames(); // Get the names used to launch the game
        public AGame CreateGame(IMessageChannel msgchan); // Create a new instance of a game and return it
    }
}
