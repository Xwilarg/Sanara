using Discord;
using System.Collections.Generic;

namespace SanaraV2.Games
{
    public class AGame
    {
        protected AGame(ITextChannel chan, Config config)
        {
            _chan = chan;
            _config = config;
            _contributors = new List<ulong>();
            _saveName = config.gameName + (config.difficulty == Difficulty.Easy ? "-easy" : "");
            _score = 0;
        }

        private ITextChannel    _chan; // Channel where the game is
        private Config          _config; // Game configuration
        private List<ulong>     _contributors; // Ids of the users that contributed to the current score
        private string          _saveName; // Name the game will have in the db
        private int             _score; // Current score
    }
}
