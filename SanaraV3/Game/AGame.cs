using Discord;
using Discord.WebSocket;
using DiscordUtils;
using SanaraV3.Exception;
using SanaraV3.Game.PostMode;
using SanaraV3.Game.Preload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Game
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
                "If the game break, you can use the 'Cancel' command to cancel it.\n" +
                "You can cooperate with other players to find the answers." +
                (_postMode is AudioMode ? "\nYou can listen again to the audio using the 'Replay' command." : ""));

            _messages = new List<SocketUserMessage>();

            _contributors = new List<ulong>();
            _score = 0;

            StaticObjects.Website?.AddGameAsync(_gameName, _argument);
        }

        protected abstract string GetPostInternal(); // Get next post
        protected abstract Task CheckAnswerInternalAsync(string answer); // Check if user answer is right
        protected abstract string GetAnswer(); // Get the right answer (to display when we loose)
        protected abstract int GetGameTime(); // The timer an user have to answer
        protected abstract string GetRules();
        protected abstract string GetSuccessMessage(); // Congratulation message, empty string to ignore
        protected virtual void DisposeInternal() // By default there isn't much to dispose but some child class might need it
        { }
        protected virtual string GetHelp() // Does display help in format X*****
            => null;
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
                    if (!(_postMode is AudioMode))
                        await _postMode.PostAsync(_textChan, _current, this);
                    else
                        _ = Task.Run(async () => { await _postMode.PostAsync(_textChan, _current, this); }); // We don't wait for the audio to finish for audio games // TODO: That also means we don't handle exceptions
                    if (GetHelp() != null)
                    {
                        var answer = GetHelp();
                        string answerHelp = char.ToUpper(answer[0]).ToString();
                        foreach (var c in answer.Skip(1))
                        {
                            if (c == ' ')
                                answerHelp += c;
                            else
                                answerHelp += "\\*";
                        }
                        await _textChan.SendMessageAsync(answerHelp);
                    }
                } catch (GameLost e)
                {
                    await LooseAsync(e.Message);
                    return;
                } catch (System.Exception e)
                {
                    await Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                    if (nbTries == 3)
                    {
                        await LooseAsync("Failed to get something to post after 3 tries...");
                        return;
                    }
                    nbTries++;
                    continue;
                }
                break;
            } while (true);
            _lastPost = DateTime.Now; // Reset timer
            _state = GameState.RUNNING;
        }

        public void AddAnswer(SocketUserMessage msg)
        {
            lock (_messages)
            {
                _messages.Add(msg);
            }
        }

        /// <summary>
        /// Check if the user answer is valid
        /// </summary>
        public Task CheckAnswersAsync()
        {
            lock (_messages)
            {
                if (_state != GameState.RUNNING)
                    return Task.CompletedTask;
                foreach (var msg in _messages)
                {
                    try
                    {
                        CheckAnswerInternalAsync(msg.Content).GetAwaiter().GetResult();
                        string congratulation = GetSuccessMessage();
                        if (congratulation != null)
                            _textChan.SendMessageAsync(congratulation).GetAwaiter().GetResult();
                        if (!_contributors.Contains(msg.Author.Id))
                            _contributors.Add(msg.Author.Id);
                        _score++;
                        _ = Task.Run(async () => { await PostAsync(); }); // We don't wait for the post to be sent to not block the whole world
                        break; // Good answer found, no need to check the others ones
                    }
                    catch (GameLost e)
                    {
                        LooseAsync(e.Message).GetAwaiter().GetResult();
                    }
                    catch (InvalidGameAnswer e)
                    {
                        _ = Task.Run(async () =>
                        {
                            if (e.Message.Length == 0)
                                await msg.AddReactionAsync(new Emoji("❌"));
                            else
                                _textChan.SendMessageAsync(e.Message).GetAwaiter().GetResult();
                        });
                    }
                }
                _messages.Clear();
            }
            return Task.CompletedTask;
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
            await CheckAnswersAsync(); // We check the answers that were sent to be sure to not loose a game while we are still supposed to treat an answer

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

        List<SocketUserMessage> _messages;

        // SCORES
        private List<ulong> _contributors; // Users that contributed
        protected int _score;
    }
}
