using Discord;
using SanaraV3.Exceptions;
using SanaraV3.Modules.Game.PostMode;
using System;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game
{
    public abstract class AGame
    {
        protected AGame(IMessageChannel textChan, IPostMode postMode, GameSettings settings)
        {
            _state = GameState.PREPARE;
            _textChan = textChan;
            _postMode = postMode;
            _settings = settings;

            textChan.SendMessageAsync(GetRules() + "\n\nIf the game break, you can use the 'Cancel' command to cancel it.");
        }

        protected abstract string GetPostInternal(); // Get next post
        protected abstract Task CheckAnswerInternalAsync(string answer); // Check if user answer is right
        protected abstract string GetAnswer(); // Get the right answer (to display when we loose)
        protected abstract int GetGameTime(); // The timer an user have to answer
        protected abstract string GetRules();

        /// <summary>
        /// Start the game, that's where lobby management is done
        /// </summary>
        public async Task StartAsync()
        {
            if (_state != GameState.PREPARE)
                return;
            _state = GameState.RUNNING;
            await PostAsync();
        }

        private async Task PostAsync()
        {
            if (_state != GameState.RUNNING)
                return;
            _state = GameState.POSTING; // Making sure we can't loose or validate answer while we are posting a new image
            await _postMode.PostAsync(_textChan, GetPostInternal());
            _lastPost = DateTime.Now; // Reset timer
            _state = GameState.RUNNING;
        }

        /// <summary>
        /// Check if the user answer is valid
        /// </summary>
        public async Task CheckAnswerAsync(string userAnswer)
        {
            if (_state != GameState.RUNNING)
                return;
            try
            {
                await CheckAnswerInternalAsync(userAnswer);
                await PostAsync();
            }
            catch (GameLost e)
            {
                await LooseAsync(e.Message);
            }
            catch (InvalidGameAnswer e)
            {
                await _textChan.SendMessageAsync(e.Message);
            }
        }

        public async Task CancelAsync()
        {
            if (_state == GameState.LOST) // No point cancelling a game that is already lost
            {
                await _textChan.SendMessageAsync("The game is already lost.");
                return;
            }

            await LooseAsync("Game cancelled");
        }

        private async Task LooseAsync(string reason)
        {
            _state = GameState.LOST;
            await _textChan.SendMessageAsync($"You lost: {reason}\n{GetAnswer()}");
        }

        public async Task CheckTimerAsync()
        {
            if (_state != GameState.RUNNING)
                return;

            if (_lastPost.AddSeconds(GetGameTime()) < DateTime.Now) // If post time + game time is < to current time, that means the player spent too much time answering
                await LooseAsync("Time out");
        }

        /// <summary>
        /// Is the game lost
        /// </summary>
        public bool AsLost()
            => _state == GameState.LOST;

        public bool IsMyGame(ulong chanId)
            => _textChan.Id == chanId;

        private GameState       _state; // Current state of the game
        private readonly IMessageChannel _textChan; // Textual channel where the game is happening
        private readonly IPostMode       _postMode; // How things should be posted
        private DateTime        _lastPost; // Used to know when the user lost because of the time
        private readonly GameSettings    _settings; // Contains various settings about the game
    }
}
