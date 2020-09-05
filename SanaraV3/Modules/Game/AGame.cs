using Discord;
using Discord.WebSocket;
using DiscordUtils;
using SanaraV3.Exceptions;
using SanaraV3.Modules.Game.PostMode;
using SanaraV3.Modules.Game.Preload;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game
{
    public abstract class AGame : IDisposable
    {
        protected AGame(IMessageChannel textChan, IUser _, IPreload preload, IPostMode postMode, GameSettings settings)
        {
            _state = GameState.PREPARE;
            _textChan = textChan;
            if (_textChan is ITextChannel)
                _guildId = ((ITextChannel)_textChan).GuildId;
            else
                throw new CommandFailed("Games are not yet available in private message."); // TODO!
            _postMode = postMode;
            _settings = settings;

            _gameName = preload.GetGameNames()[0];
            _argument = preload.GetNameArg();

            textChan.SendMessageAsync(GetRules() + $"\n\nYou will loose if you don't answer after {GetGameTime()} seconds\n\n" +
                "If the game break, you can use the 'Cancel' command to cancel it." +
                (_postMode is AudioMode ? "\nYou can listen again to the audio using the 'Replay' command." : ""));

            _contributors = new List<ulong>();
            _score = 0;
        }

        protected abstract string GetPostInternal(); // Get next post
        protected abstract Task CheckAnswerInternalAsync(string answer); // Check if user answer is right
        protected abstract string GetAnswer(); // Get the right answer (to display when we loose)
        protected abstract int GetGameTime(); // The timer an user have to answer
        protected abstract string GetRules();
        protected abstract string GetSuccessMessage(); // Congratulation message, empty string to ignore
        protected virtual void DisposeInternal() // By default there isn't much to dispose but some child class might need it
        { }
        public void Dispose()
        {
            DisposeInternal();
        }

        public async Task ReplayAsync()
        {
            if (!(_postMode is AudioMode))
                throw new CommandFailed("Replay can only be done on audio games.");
            await _postMode.PostAsync(_textChan, _current, this);
        }

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

            _state = GameState.POSTING;

            // If somehow an error happened, we try sending a new image (up to 3 times)
            int nbTries = 0;
            do
            {
                try
                {
                    _current = GetPostInternal();
                    Task t = Task.Run(async () => { await _postMode.PostAsync(_textChan, _current, this); });
                    if (!(_postMode is AudioMode)) // We don't wait for the audio to finish for audio games
                        t.Wait();
                } catch (Exception e)
                {
                    await Utils.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                    if (nbTries == 3)
                        throw new GameLost("Failed to get something to post after 3 tries...");
                    nbTries++;
                    continue;
                }
                break;
            } while (true);
            _lastPost = DateTime.Now; // Reset timer
            _state = GameState.RUNNING;
        }

        /// <summary>
        /// Check if the user answer is valid
        /// </summary>
        public async Task CheckAnswerAsync(SocketUserMessage msg)
        {
            if (_state != GameState.RUNNING)
                return;
            try
            {
                await CheckAnswerInternalAsync(msg.Content);
                string congratulation = GetSuccessMessage();
                if (congratulation != null)
                    await _textChan.SendMessageAsync(congratulation);
                if (!_contributors.Contains(msg.Author.Id))
                    _contributors.Add(msg.Author.Id);
                _score++;
                await PostAsync();
            }
            catch (GameLost e)
            {
                await LooseAsync(e.Message);
            }
            catch (InvalidGameAnswer e)
            {
                if (e.Message.Length == 0)
                    await msg.AddReactionAsync(new Emoji("❌"));
                else
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
            int bestScore = await StaticObjects.Db.GetGameScoreAsync(_guildId, _gameName, _argument);
            string scoreSentence;
            if (_score < bestScore) scoreSentence = $"You didn't beat your best score of {bestScore} with your score of {_score}.";
            else if (_score == bestScore) scoreSentence = $"You equalized your best score with a score of {bestScore}.";
            else
            {
                await StaticObjects.Db.SaveGameScoreAsync(_guildId, _score, _contributors, _gameName, _argument);
                scoreSentence = $"You best your best score of {bestScore} with a new score of {_score}!";
            }
            await _textChan.SendMessageAsync($"You lost: {reason}\n{GetAnswer()}\n\n" + scoreSentence);
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

        private string _gameName; // Name of the game
        private string _argument; // Game option (audio, shadow, etc...)
        private GameState _state; // Current state of the game
        private readonly ulong _guildId;
        private readonly IMessageChannel _textChan; // Textual channel where the game is happening
        private readonly IPostMode _postMode; // How things should be posted
        private DateTime _lastPost; // Used to know when the user lost because of the time
        private readonly GameSettings _settings; // Contains various settings about the game
        private string _current; // Current value, used for Replay command

        // SCORES
        private List<ulong> _contributors; // Users that contributed
        private int _score;
    }
}
