using System.Threading.Tasks;

namespace SanaraV3.Modules.Game
{
    public abstract class AGame
    {
        public AGame()
        {
            _state = GameState.PREPARE;
        }

        public async Task Start()
        {
            if (_state != GameState.PREPARE)
                return;
            _state = GameState.RUNNING;
        }

        public async Task PostAsync()
        {
            if (_state != GameState.RUNNING)
                return;
        }

        private GameState _state; // Current state of the game
    }
}
