using Discord;
using SanaraV3.Modules.Game.PostMode;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game
{
    public abstract class AGame
    {
        public AGame(IMessageChannel textChan, IPostMode postMode)
        {
            _state = GameState.RUNNING;
            _textChan = textChan;
            _postMode = postMode;
        }

        protected abstract string GetPostInternal();

        /// <summary>
        /// Start the game, that's where lobby management is done
        /// </summary>
        public async Task Start()
        {
            if (_state != GameState.PREPARE)
                return;
        }

        public async Task PostAsync()
        {
            if (_state != GameState.RUNNING)
                return;


            await _postMode.PostAsync(_textChan, GetPostInternal());
        }

        /// <summary>
        /// Check if the user answer is valid
        /// </summary>
        public async Task CheckAnswerAsync(string userAnswer)
        {

        }

        /// <summary>
        /// Is the game lost
        /// </summary>
        public bool IsLost()
            => _state == GameState.LOST;

        public bool IsMyGame(ulong chanId)
            => _textChan.Id == chanId;

        private GameState       _state; // Current state of the game
        private IMessageChannel _textChan; // Textual channel where the game is happening
        private IPostMode       _postMode;
    }
}
