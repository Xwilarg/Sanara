using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using Sanara.Database;
using Sanara.Exception;
using Sanara.Game.MultiplayerMode;
using Sanara.Game.PostMode;
using Sanara.Game.Preload;
using Sanara.Module.Command;

namespace Sanara.Game
{
    public abstract class AGame : IDisposable
    {
        protected AGame(IServiceProvider provider, CommonMessageChannel textChan, CommonUser _, IPreload preload, IPostMode postMode, IMultiplayerMode versusMode, GameSettings settings)
        {
            _provider = provider;

            _state = GameState.Prepare;
            _textChan = textChan;
            if (_textChan is ITextChannel)
                _guildId = ((ITextChannel)_textChan).GuildId;
            else
                throw new CommandFailed("Games are not yet available in private message."); // TODO!
            _postMode = postMode;
            _versusMode = versusMode;
            _lobby = settings.Lobby;
            _isCustomGame = settings.IsCustomGame;

            _gameName = preload.Name;
            _argument = null;// preload.GetNameArg();
            _messages = new List<IContext>();
            _preload = preload;

            _contributors = new List<string>();
            _score = 0;

            _lobby.InitVersusRules(_versusMode.GetRules());
        }

        protected abstract string[] GetPostInternal(); // Get next post
        protected abstract Task CheckAnswerInternalAsync(IContext answer); // Check if user answer is right
        protected abstract string GetAnswer(); // Get the right answer (to display when we loose)
        protected abstract int GetGameTime(); // The timer an user have to answer
        protected abstract string GetSuccessMessage(CommonUser user); // Congratulation message, empty string to ignore
        protected virtual void DisposeInternal() // By default there isn't much to dispose but some child class might need it
        { }
        protected virtual string GetHelp() // Does display help in format X*****
            => null;
        public void Dispose()
        {
            DisposeInternal();
        }

        public Lobby? GetLobby()
        {
            if (_state == GameState.Prepare)
            {
                return _lobby;
            }
            return null;
        }

        public bool CanPlay(CommonUser user)
            => _lobby.ContainsUser(user);

        public async Task ReplayAsync()
        {
            if (_postMode is not AudioMode)
                throw new CommandFailed("Replay can only be done on audio games.");
            await _postMode.PostAsync(_provider, _textChan, _current, this);
        }

        public async Task StartAsync(IContext ctx)
        {
            if (_state != GameState.Prepare)
                return;

            if (_lobby.MultiplayerType == MultiplayerType.VERSUS)
            {
                _versusMode.Init(_provider.GetRequiredService<Random>(), _lobby.GetUsers());
            }
            await _provider.GetRequiredService<Db>().AddGamePlayerAsync(_isCustomGame ? "custom" : _gameName, _argument, _lobby.GetUserCount(), _lobby.MultiplayerType);

            _state = GameState.Ready;

            await ctx.ReplyAsync(string.Join(", ", _lobby.GetAllMentions()) + " the game is starting.");
            await PostAsync(null);
        }

        private string GetPostContent()
        {
            string str = "";
            if (GetHelp() != null)
               str += "\n" + GetHelp();
            if (_lobby.MultiplayerType == MultiplayerType.VERSUS)
            {
                var multiInfo = _versusMode.PrePost();
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
            if (_state != GameState.Running && _state != GameState.Ready)
                return;

            _state = GameState.Posting;

            // If somehow an error happened, we try sending a new image (up to 3 times)
            int nbTries = 0;
            do
            {
                string[] currentPostDebug = Array.Empty<string>(); // We save the current post here to send it to Sentry if an exception occurs
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
                        if (_postMode is not TextMode && introMsg != null)
                            await _textChan.SendMessageAsync(introMsg);
                        foreach (var tmp in post)
                        {
                            _current = tmp;
                            if (_postMode is AudioMode)
                                _ = Task.Run(async () => { await _postMode.PostAsync(_provider, _textChan, _current, this); }); // We don't wait for the audio to finish for audio games // TODO: That also means we don't handle exceptions
                            else if (_postMode is TextMode)
                            {
                                await _postMode.PostAsync(_provider, _textChan, (introMsg == null ? "" : introMsg) + _current + postContent, this);
                                introMsg = null;
                            }
                            else
                                await _postMode.PostAsync(_provider, _textChan, _current, this);
                        }
                    }
                    if (_postMode is not TextMode)
                    {
                        if (postContent != "")
                            await _textChan.SendMessageAsync(postContent);
                    }
                } catch (GameLost e)
                {
                    _state = GameState.Running;
                    await LooseAsync(e.Message, true);
                    return;
                } catch (System.Exception e)
                {
                    var specifierException = new System.Exception("Error while posting for " + _gameName + ", tried to post " + currentPostDebug.Length + " elements."
                        + (currentPostDebug.Length == 0 ? "" : "First element is: " + currentPostDebug[0]));
                    await Log.LogErrorAsync(e, null);
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
            _state = GameState.Running;
        }

        public void AddAnswer(IContext msg)
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
                if (_state != GameState.Running && _state != GameState.Lost)
                    return Task.CompletedTask;
                try
                {
                    foreach (var msg in _messages)
                    {
                        var task = Task.Run(() =>
                        {
                            if (_lobby.MultiplayerType == MultiplayerType.VERSUS)
                                _versusMode.PreAnswerCheck(msg.User);
                            CheckAnswerInternalAsync(msg).GetAwaiter().GetResult();
                        });
                        try
                        {
                            if (task.Wait(5000)) // Not supposed to timeout, but we just put a timer of 5s to be sure
                            {
                                string introMsg = GetSuccessMessage(msg.User);
                                if (!_contributors.Contains(msg.User.Id))
                                    _contributors.Add(msg.User.Id);
                                _score++;
                                if (_state == GameState.Lost)
                                    _state = GameState.Running;
                                _ = Task.Run(async () => { await PostAsync(introMsg); }); // We don't wait for the post to be sent to not block the whole world
                                if (_lobby.MultiplayerType == MultiplayerType.VERSUS)
                                    _versusMode.AnswerIsCorrect(msg.User);
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
                                if (_state == GameState.Running)
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
                                        await msg.ReplyAsync(e.InnerException.Message);
                                });
                            }
                            else
                            {
                                _ = Task.Run(async () =>
                                {
                                    await Log.LogErrorAsync(e, null);
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
            if (_state == GameState.Lost) // No point cancelling a game that is already lost
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
            if (_lobby.MultiplayerType == MultiplayerType.VERSUS) // Multiplayer games
            {
                string msg;
                bool canLoose = _versusMode.CanLooseAuto();
                if (canLoose)
                    msg = $"You lost: {reason}";
                else
                    msg = $"Nobody found the answer";
                if (bypassMultiplayerCheck || (canLoose && !_versusMode.Loose()))
                {
                    string outro = _versusMode.GetOutroLoose();
                    await _textChan.SendMessageAsync(msg + (canLoose ? "\n" + GetAnswer() : "") + $"\n{_versusMode.GetWinner()} won" + (outro != null ? "\n" + outro : ""));
                    _state = GameState.Lost;
                    await CreateReplayLobbyAsync();
                }
                else
                {
                    await PostAsync(msg + (!canLoose ? "\n" + GetAnswer() : "") + "\n");
                }
                return;
            }
            _state = GameState.Lost;

            await CheckAnswersAsync(); // We check the answers that were sent to be sure to not loose a game while we are still supposed to treat an answer

            if (_state != GameState.Lost)
                return;

            string scoreSentence = "";
            if (_lobby.MultiplayerType != MultiplayerType.VERSUS && !_isCustomGame) // Score aren't saved in multiplayer games
            {
                var db = _provider.GetRequiredService<Db>();
                int bestScore = db.GetGameScore(_guildId, _gameName, _argument);

                if (_score < bestScore) scoreSentence = $"You didn't beat the guild best score of {bestScore} with your score of {_score}.";
                else if (_score == bestScore) scoreSentence = $"You equalized the guild best score with a score of {bestScore}.";
                else
                {
                    await db.SaveGameScoreAsync(_guildId, _score, _contributors, _gameName, _argument);
                    scoreSentence = $"You have beat the guild best score of {bestScore} with a new score of {_score}!";

                    var guild = db.GetGuild(_guildId);
                    string fullName = _argument == null ? _gameName : (_gameName + "-" + _argument);
                    int myScore = guild.GetScore(db.GetCacheName(fullName));
                    var scores = db.GetAllScores(fullName);
                    scoreSentence += "\nYou are ranked #" + (scores.Count(x => x > myScore) + 1) + " out of " + scores.Count;
                }
            }
            await _textChan.SendMessageAsync($"You lost: {reason}\n{GetAnswer()}\n\n" + scoreSentence);
            await CreateReplayLobbyAsync();
        }

        private async Task CreateReplayLobbyAsync()
        {
            var replayLobby = _provider.GetRequiredService<GameManager>().AddReplayLobby(_textChan, _preload, _lobby);

            var buttons = new ComponentBuilder()
                .WithButton(label: "Ready/Unready", customId: $"replay/ready", style: ButtonStyle.Success)
                .WithButton(label: "Delete", customId: $"replay/delete", style: ButtonStyle.Danger);
            replayLobby.Message = await _textChan.SendMessageAsync(embed: replayLobby.GetEmbed(), components: buttons.Build());
        }

        public async Task CheckTimerAsync()
        {
            if (_state != GameState.Running)
                return;

            if (_lastPost.AddSeconds(GetGameTime()) < DateTime.Now) // If post time + game time is < to current time, that means the player spent too much time answering
                await LooseAsync("Time out", false);
        }

        /// <summary>
        /// Is the game lost
        /// </summary>
        public bool AsLost()
            => _state == GameState.Lost || (_state == GameState.Prepare && _lobby.HasExpired);

        public bool IsMyGame(string chanId)
            => _textChan.Id == chanId;

        private string _gameName; // Name of the game
        private string _argument; // Game option (audio, shadow, etc...)
        private GameState _state; // Current state of the game
        private readonly ulong _guildId;
        private readonly CommonMessageChannel _textChan; // Textual channel where the game is happening
        private readonly IPostMode _postMode; // How things should be posted
        private DateTime _lastPost; // Used to know when the user lost because of the time
        private string _current; // Current value, used for Replay command
        private IPreload _preload;

        List<IContext> _messages;

        // SCORES
        private List<string> _contributors; // Users that contributed
        protected int _score;
        private bool _isCustomGame;

        // MULTIPLAYER
        protected Lobby _lobby;
        private const int _lobbyTimer = 30;
        protected IMultiplayerMode _versusMode;

        protected IServiceProvider _provider;
    }
}