using Discord;
using Discord.WebSocket;
using SanaraV3.Exception;
using SanaraV3.Game.MultiplayerMode;
using SanaraV3.Game.PostMode;
using SanaraV3.Game.Preload;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV3.Game
{
    public abstract class AGame : IDisposable
    {
        protected AGame(IMessageChannel textChan, IUser _, IPreload preload, IPostMode postMode, IMultiplayerMode multiplayerMode, GameSettings settings)
        {
            _state = GameState.PREPARE;
            _textChan = textChan;
            if (_textChan is ITextChannel)
                _guildId = ((ITextChannel)_textChan).GuildId;
            else
                throw new CommandFailed("Games are not yet available in private message."); // TODO!
            _postMode = postMode;
            _multiplayerMode = multiplayerMode;
            _lobby = settings.Lobby;
            _doesSaveScore = settings.DoesSaveScore;

            _gameName = preload.GetGameNames()[0];
            _argument = preload.GetNameArg();

            textChan.SendMessageAsync(preload.GetRules() +
                (_lobby != null ? "\n*Multiplayer rules:* " + _multiplayerMode.GetRules() : "") +
                $"\n\nYou will loose if you don't answer after {GetGameTime()} seconds\n\n" +
                "If the game break, you can use the 'Cancel' command to cancel it.\n" +
                "You can cooperate with other players to find the answers." +
                (_postMode is AudioMode ? "\nYou can listen again to the audio using the 'Replay' command." : "") +
                (_lobby != null ? "\n\n**You can join the lobby for the game by doing the 'Join' command**\nThe game will automatically start in " + _lobbyTimer + " seconds, you can start it right away using the 'Start' command" : "")).GetAwaiter().GetResult();

            _messages = new List<SocketUserMessage>();

            _contributors = new List<ulong>();
            _score = 0;

            if (StaticObjects.Website != null)
                Task.Run(() => StaticObjects.Website.AddGameAsync(_gameName, _argument));
        }

        protected abstract string[] GetPostInternal(); // Get next post
        protected abstract Task CheckAnswerInternalAsync(SocketUserMessage answer); // Check if user answer is right
        protected abstract string GetAnswer(); // Get the right answer (to display when we loose)
        protected abstract int GetGameTime(); // The timer an user have to answer
        protected abstract string GetSuccessMessage(IUser user); // Congratulation message, empty string to ignore
        protected virtual void DisposeInternal() // By default there isn't much to dispose but some child class might need it
        { }
        protected virtual string GetHelp() // Does display help in format X*****
            => null;
        public void Dispose()
        {
            DisposeInternal();
        }

        public bool IsMultiplayerGame()
            => _lobby != null;

        public async Task ReplayAsync()
        {
            if (!(_postMode is AudioMode))
                throw new CommandFailed("Replay can only be done on audio games.");
            await _postMode.PostAsync(_textChan, _current, this);
        }

        public GameState GetState()
            => _state;

        public async Task StartWhenReadyAsync()
        {
            if (_lobby != null) // Multiplayer game
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(_lobbyTimer * 1000);
                    await StartAsync();
                });
            }
            else
                await StartAsync();
        }

        public bool Join(IUser user)
        {
            if (_state != GameState.PREPARE)
                return false;

            return _lobby.AddUser(user);
        }

        public bool IsLobbyOwner(IUser user)
            => _lobby.IsHost(user);

        public async Task StartAsync()
        {
            if (_state != GameState.PREPARE)
                return;

            string introMsg = null;

            if (_lobby != null) // Multiplayer game
            {
                if (_lobby.GetUserCount() < 2)
                {
                    _state = GameState.LOST;
                    await _textChan.SendMessageAsync("The game was cancelled because there wasn't enough players (at least 2 are required)");
                    return;
                }
                _multiplayerMode.Init(_lobby.GetUsers());
                introMsg = string.Join(", ", _lobby.GetAllMentions()) + " the game is starting.";

                if (StaticObjects.Website != null)
                    await StaticObjects.Website.AddGamePlayerAsync(_gameName, _argument, _lobby.GetUserCount());
            }
            else if (StaticObjects.Website != null)
                await StaticObjects.Website?.AddGamePlayerAsync(_gameName, _argument, 1);

            _state = GameState.READY;
            await PostAsync(introMsg);
        }

        private string GetPostContent()
        {
            string str = "";
            if (GetHelp() != null)
               str += "\n" + GetHelp();
            if (_lobby != null)
            {
                var multiInfo = _multiplayerMode.PrePost();
                if (multiInfo != null)
                    str += "\n" + multiInfo;
            }
            return str;
        }

        /// <summary>
        /// Post a new image/text/other for the game
        /// </summary>
        /// <param name="introMsg">Message to be sent before the game content, null if none</param>
        private async Task PostAsync(string introMsg)
        {
            if (_state != GameState.RUNNING && _state != GameState.READY)
                return;

            _state = GameState.POSTING;

            // If somehow an error happened, we try sending a new image (up to 3 times)
            int nbTries = 0;
            do
            {
                string[] currentPostDebug = new string[0]; // We save the current post here to send it to Sentry if an exception occurs
                try
                {
                    _messages.Clear();
                    var post = GetPostInternal();
                    currentPostDebug = post;
                    var postContent = GetPostContent();
                    if (post.Length == 0 && _postMode is TextMode)
                    {
                        var output = (introMsg == null ? "" : introMsg) + postContent;
                        if (output != "")
                            await _textChan.SendMessageAsync(output);
                    }
                    else
                    {
                        if (!(_postMode is TextMode) && introMsg != null)
                            await _textChan.SendMessageAsync(introMsg);
                        foreach (var tmp in post)
                        {
                            _current = tmp;
                            if (_postMode is AudioMode)
                                _ = Task.Run(async () => { await _postMode.PostAsync(_textChan, _current, this); }); // We don't wait for the audio to finish for audio games // TODO: That also means we don't handle exceptions
                            else if (_postMode is TextMode)
                            {
                                await _postMode.PostAsync(_textChan, (introMsg == null ? "" : introMsg) + _current + postContent, this);
                                introMsg = null;
                            }
                            else
                                await _postMode.PostAsync(_textChan, _current, this);
                        }
                    }
                    if (!(_postMode is TextMode))
                    {
                        if (postContent != "")
                            await _textChan.SendMessageAsync(postContent);
                    }
                } catch (GameLost e)
                {
                    _state = GameState.RUNNING;
                    await LooseAsync(e.Message, true);
                    return;
                } catch (System.Exception e)
                {
                    var specifierException = new System.Exception("Error while posting for " + _gameName + ", tried to post " + currentPostDebug.Length + " elements."
                        + (currentPostDebug.Length == 0 ? "" : "First element is: " + currentPostDebug[0]));
                    await Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                    if (nbTries == 3)
                    {
                        await LooseAsync("Failed to get something to post after 3 tries...", true);
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
                if (_state != GameState.RUNNING && _state != GameState.LOST)
                    return Task.CompletedTask;
                try
                {
                    foreach (var msg in _messages)
                    {
                        var task = Task.Run(() =>
                        {
                            if (_lobby != null)
                                _multiplayerMode.PreAnswerCheck(msg.Author);
                            CheckAnswerInternalAsync(msg).GetAwaiter().GetResult();
                        });
                        try
                        {
                            if (task.Wait(5000)) // Not supposed to timeout, but we just put a timer of 5s to be sure
                            {
                                string introMsg = GetSuccessMessage(msg.Author);
                                if (!_contributors.Contains(msg.Author.Id))
                                    _contributors.Add(msg.Author.Id);
                                _score++;
                                if (_state == GameState.LOST)
                                    _state = GameState.RUNNING;
                                _ = Task.Run(async () => { await PostAsync(introMsg); }); // We don't wait for the post to be sent to not block the whole world
                                if (_lobby != null)
                                    _multiplayerMode.AnswerIsCorrect(msg.Author);
                                break; // Good answer found, no need to check the others ones
                            }
                            else
                            {
                                _ = Task.Run(async () =>
                                {
                                    await msg.AddReactionAsync(new Emoji("❔"));
                                });
                            }
                        }
                        catch (AggregateException e)
                        {
                            if (e.InnerException is GameLost)
                            {
                                if (_state == GameState.RUNNING)
                                    LooseAsync(e.InnerException.Message, false).GetAwaiter().GetResult();
                                break;
                            }
                            else if (e.InnerException is InvalidGameAnswer)
                            {
                                _ = Task.Run(async () =>
                                {
                                    if (e.InnerException.Message.Length == 0)
                                        await msg.AddReactionAsync(new Emoji("❌"));
                                    else
                                        _textChan.SendMessageAsync(e.InnerException.Message).GetAwaiter().GetResult();
                                });
                            }
                            else
                            {
                                _ = Task.Run(async () =>
                                {
                                    await Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.InnerException.Source, e.InnerException.Message, e.InnerException));
                                    await msg.AddReactionAsync(new Emoji("🕷"));
                                });
                            }
                        }
                    }
                    _messages.Clear();
                }
                catch (InvalidOperationException) // This is only done when we don't need the current messages anyway
                { }
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

            await LooseAsync("Game cancelled", true);
        }

        /// <summary>
        /// Loose the current game
        /// </summary>
        /// <param name="reason">Why the game was lost</param>
        /// <param name="bypassMultiplayerCheck">If true, will stop the whole game even in multiplayer. If false, will just make the current player loose</param>
        /// <returns></returns>
        private async Task LooseAsync(string reason, bool bypassMultiplayerCheck)
        {
            if (_lobby != null) // Multiplayer games
            {
                string msg;
                bool canLoose = _multiplayerMode.CanLooseAuto();
                if (canLoose)
                    msg = $"You lost: {reason}";
                else
                    msg = $"Nobody found the answer";
                if (bypassMultiplayerCheck || (canLoose && !_multiplayerMode.Loose()))
                {
                    string outro = _multiplayerMode.GetOutroLoose();
                    await _textChan.SendMessageAsync(msg + (canLoose ? "\n" + GetAnswer() : "") + $"\n{_multiplayerMode.GetWinner()} won" + (outro != null ? "\n" + outro : ""));
                    _state = GameState.LOST;
                }
                else
                {
                    await PostAsync(msg + (!canLoose ? "\n" + GetAnswer() : "") + "\n");
                }
                return;
            }
            _state = GameState.LOST;

            await CheckAnswersAsync(); // We check the answers that were sent to be sure to not loose a game while we are still supposed to treat an answer

            if (_state != GameState.LOST)
                return;

            int bestScore = StaticObjects.Db.GetGameScore(_guildId, _gameName, _argument);

            string scoreSentence = "";
            if (_lobby == null || !_doesSaveScore) // Score aren't saved in multiplayer games
            {
                if (_score < bestScore) scoreSentence = $"You didn't beat your best score of {bestScore} with your score of {_score}.";
                else if (_score == bestScore) scoreSentence = $"You equalized your best score with a score of {bestScore}.";
                else
                {
                    await StaticObjects.Db.SaveGameScoreAsync(_guildId, _score, _contributors, _gameName, _argument);
                    scoreSentence = $"You have beat your best score of {bestScore} with a new score of {_score}!";
                }
            }
            await _textChan.SendMessageAsync($"You lost: {reason}\n{GetAnswer()}\n\n" + scoreSentence);
        }

        public async Task CheckTimerAsync()
        {
            if (_state != GameState.RUNNING)
                return;

            if (_lastPost.AddSeconds(GetGameTime()) < DateTime.Now) // If post time + game time is < to current time, that means the player spent too much time answering
                await LooseAsync("Time out", false);
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
        private string _current; // Current value, used for Replay command

        List<SocketUserMessage> _messages;

        // SCORES
        private List<ulong> _contributors; // Users that contributed
        protected int _score;
        private bool _doesSaveScore;

        // MULTIPLAYER
        protected MultiplayerLobby _lobby;
        private const int _lobbyTimer = 30;
        protected IMultiplayerMode _multiplayerMode;
    }
}