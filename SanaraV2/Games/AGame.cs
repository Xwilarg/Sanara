/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.

using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Games
{
    public abstract class AGame
    {
        protected AGame(ITextChannel chan, List<string> dictionnary, Config config, ulong playerId, bool ignoreDictionnarycheck = false)
        {
            _chan = chan;
            if (!ignoreDictionnarycheck && (dictionnary == null || dictionnary.Count < 200)) // Dictionnary failed to load
                throw new NoDictionnaryException();
            _dictionnary = dictionnary != null ? new List<string>(dictionnary) : null; // We create a new one to be sure to not modify the common one
            _contributors = new List<ulong>();
            _saveName = config.gameName + (config.difficulty == Difficulty.Easy ? "-easy" : "") + (config.isFull ? "-full" : "") + (config.isCropped ? "-cropped" : "") + (config.isShaded != APreload.Shadow.None ? "-shaded" : "");
            _gameName = new CultureInfo("en-US").TextInfo.ToTitleCase(config.gameName);
            _score = 0;
            _postImage = false;
            _checkingAnswer = false;
            _gameState = GameState.WaitingForPlayers;
            _startTime = DateTime.Now;
            _timer = config.refTime * (int)config.difficulty;
            _lobby = (config.isMultiplayer == APreload.Multiplayer.MultiOnly ? new MultiplayerLobby(playerId) : null);
            _isFound = false;
            _isCropped = config.isCropped;
            _isShaded = config.isShaded;
            _multiType = config.multiplayerType;
            _bestOfScore = new Dictionary<string, int>();
            _bestOfTries = new Dictionary<string, int>();
            _bestOfRemainingRounds = 5;
            Init();
        }

        public bool IsSelf(ulong chanId) // Allow to check if a game is running in this channel
            => _chan.Id == chanId;

        public void Cancel()
        {
            _gameState = GameState.Lost;
        }

        protected abstract void Init(); // User must not use the ctor, they must init things in this function (because GetPost is called from this ctor, who is called before the child ctor)
        protected abstract Task<string[]> GetPostAsync();
        protected abstract PostType GetPostType();
        protected abstract Task<string> GetCheckCorrectAsync(string userAnswer); // Return null if suceed, else return error message
        protected abstract Task<string> GetLoose();
        protected abstract bool CongratulateOnGuess(); // Say "Congratulation you found the right answer" on a guess
        protected abstract string Help(); // null is no help
        protected virtual int? GetMaximumMultiplayer() // Maximum number of player if the game allow multiplayer
            => null;
        protected virtual async Task NextTurnInternal() // Called at each turn in case the game want to check something, only for multiplayer games
        { }

        public string GetName()
            => _gameName;

        public bool IsReady() // Check if the game is ready to start (if multiplayer, have to wait for people to join)
            => _lobby != null ? _lobby.IsReady() : true;

        public bool HaveEnoughPlayer() // Check if the game have enough player to start (multiplayer games need at least 2 people)
            => _lobby != null ? _lobby.HaveEnoughPlayer() : true;

        public bool HaveMultiplayerLobby() // Is the current game multiplayer (true) or solo (false)
            => _lobby != null;

        public bool IsFull()
            => GetMaximumMultiplayer() != null && _lobby.GetNumberPlayers() == GetMaximumMultiplayer();

        public async Task DisplayCantStart()
        {
            await _chan.SendMessageAsync(Sentences.LobbyNotEnoughPlayerFatal(_chan.GuildId));
            _gameState = GameState.Lost;
        }

        // Should only be called if the game is in WaitingForPlayers state, call IsWaitingForPlayers() to be sure
        // Also make sure that the game can receive any player with HaveEnoughPlayer()
        // And make sure that he isn't already in with IsPlayerInLobby(ulong)
        public void AddPlayerToLobby(ulong player)
            => _lobby.AddPlayer(player);

        // Make sure the player is in the lobby using IsPlayerInLobby(ulong)
        public void RemovePlayerFromLobby(ulong player)
            => _lobby.RemovePlayer(player);

        public bool IsPlayerInLobby(ulong playerId)
            => _lobby.IsPlayerIn(playerId);

        public bool IsLobbyEmpty()
            => _lobby.IsLobbyEmpty();

        protected string GetPlayerName(int index)
            => _lobby.GetName(index);

        protected string GetTurnName()
            => _lobby.GetTurnName();

        public async Task Start()
        {
            if (_gameState != GameState.WaitingForPlayers) // In case someone use the 'Start' command right when the game was about to be launched by itself
                return;
            _gameState = GameState.Running;
            if (HaveMultiplayerLobby())
            {
                // Setup for multiplayer:
                if (await _lobby.LoadNames(_chan)) // We try to load the nickname of everyone in the lobby
                    await _chan.SendMessageAsync(_lobby.GetReadyMessage(_chan.GuildId));
                else
                {
                    _gameState = GameState.Lost;
                    await _chan.SendMessageAsync(Sentences.LobbyLeftChannel(_chan.GuildId));
                }
                if (_multiType == APreload.MultiplayerType.Elimination)
                    await PostText(Sentences.AnnounceTurn(_chan.GuildId, _lobby.GetTurnName()));
            }
            await PostAsync();
        }

        public async Task PostAsync()
        {
            if (_gameState != GameState.Running)
                return;
            _postImage = true;
            int counter = 0;
            while (true)
            {
                if (_gameState != GameState.Running) // Can happen if the game is canceled
                    return;
                string finding = ""; // For error handling
                try
                {
                    PostType type = GetPostType();
                    if (type == PostType.Text)
                        foreach (string s in await GetPostAsync())
                        {
                            if (s == null)
                                continue;
                            finding = s;
                            await PostText(s);
                        }
                    else if (type == PostType.Url)
                        foreach (string s in await GetPostAsync())
                        {
                            if (s == null)
                                continue;
                            finding = s;
                            using (HttpClient hc = new HttpClient())
                            {
                                var answer = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Head, s));
                                if (!answer.IsSuccessStatusCode)
                                    throw new HttpRequestException("Invalid code " + answer.StatusCode);
                            }
                            await PostFromUrl(s);
                        }
                    else
                        foreach (string s in await GetPostAsync())
                        {
                            if (s == null)
                                continue;
                            finding = s;
                            await PostFromLocalPath(s);
                            File.Delete(s);
                        }
                    _startTime = DateTime.Now;
                    _isFound = false;
                    break;
                }
                catch (LooseException le)
                {
                    _postImage = false;
                    await LooseAsync(le.Message);
                    break;
                }
                catch (Exception e)
                {
                    counter++;
                    string msg = "Error posting " + finding;
                    if (counter == 3)
                    {
                        await _chan.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Color = Color.Red,
                            Title = e.GetType().ToString(),
                            Description = Sentences.ExceptionGameStop(_chan.GuildId),
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = e.Message
                            }
                        }.Build());
                        await Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, new GameException(msg, e)));
                        _postImage = false;
                        await LooseAsync(null);
                        break;
                    }
                    else
                    {
                        await _chan.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Color = Color.Orange,
                            Title = e.GetType().ToString(),
                            Description = Sentences.ExceptionGame(_chan.GuildId, e.Message),
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = e.Message
                            }
                        }.Build());
                        await Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, new GameException(msg, e)));
                    }
                }
            }
            string help = Help();
            if (help != null)
                await PostText(help);
            _postImage = false;
        }

        public async Task CheckCorrectAsync(IUser user, string userAnswer)
        {
            if (_gameState != GameState.Running)
                return;
            _checkingAnswer = true;
            if (HaveMultiplayerLobby()) // If is in multiplayer
            {
                if (!_lobby.IsPlayerIn(user.Id)) // Player isn't in the multiplayer lobby
                    return;
                // Check if it's the player's turn (elimination mode only)
                else if (_multiType == APreload.MultiplayerType.Elimination && !_lobby.IsMyTurn(user.Id))
                {
                    await PostText(Sentences.AnnounceTurnError(_chan.GuildId, _lobby.GetTurnName()));
                    _checkingAnswer = false;
                    return;
                }
                // Check if the player is out of tries
                else if (_multiType == APreload.MultiplayerType.BestOf && (!_bestOfTries.ContainsKey(user.ToString()) || _bestOfTries[user.ToString()] == nbMaxTry))
                {
                    await PostText(Sentences.OutOfTries(_chan.GuildId));
                    _checkingAnswer = false;
                    return;
                }
            }
            if (_postImage) // Image is being posted
            {
                await PostText(Sentences.WaitImage(_chan.GuildId));
                _checkingAnswer = false;
                return;
            }
            string error;
            try
            {
                error = await GetCheckCorrectAsync(userAnswer);
            }
            catch (Exception e)
            {
                await _chan.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = e.GetType().ToString(),
                    Description = Sentences.ExceptionGameCheck(_chan.GuildId),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = e.Message
                    }
                }.Build());
                await Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                await LooseAsync(null);
                return;
            }
            if (error != null)
            {
                if (HaveMultiplayerLobby() && _multiType == APreload.MultiplayerType.BestOf)
                {
                    if (_bestOfTries.ContainsKey(user.ToString())) _bestOfTries[user.ToString()]++;
                    else _bestOfTries.Add(user.ToString(), 1);
                    error += Environment.NewLine + Sentences.TurnsRemaining(_chan.GuildId, _bestOfTries[user.ToString()], user.ToString());
                }
                if (error != "")
                    await PostText(error);
                _checkingAnswer = false;
                return;
            }
            if (!_contributors.Contains(user.Id))
                _contributors.Add(user.Id);
            string finalStr = AnnounceNextTurnInternal();
            if (CongratulateOnGuess())
                finalStr += Sentences.GuessGood(_chan.GuildId);
            if (HaveMultiplayerLobby())
            {
                if (_multiType == APreload.MultiplayerType.Elimination)
                {
                    await NextTurn();
                    if (finalStr != "")
                        finalStr += Environment.NewLine;
                    finalStr += Sentences.AnnounceTurn(_chan.GuildId, _lobby.GetTurnName());
                }
                else if (_multiType == APreload.MultiplayerType.BestOf)
                {
                    if (_bestOfScore.ContainsKey(user.ToString())) _bestOfScore[user.ToString()]++;
                    else _bestOfScore.Add(user.ToString(), 1);
                    _bestOfTries = new Dictionary<string, int>();
                    finalStr += Environment.NewLine + Sentences.CurrentScore(_chan.GuildId) + Environment.NewLine;
                    foreach (var name in _lobby.GetFullNames())
                    {
                        if (_bestOfScore.ContainsKey(name))
                            finalStr += name + ": " + _bestOfScore[name] + Environment.NewLine;
                        else
                            finalStr += name + ": 0" + Environment.NewLine;
                    }
                }
            }
            if (_gameState != GameState.Running || _isFound)
                return;
            _isFound = true;
            if (!string.IsNullOrWhiteSpace(finalStr))
                await PostText(finalStr);
            _score++;
            await PostAsync();
            _checkingAnswer = false;
        }

        protected virtual string AnnounceNextTurnInternal()
        {
            return "";
        }

        protected async Task ForceNextTurn()
        {
            await NextTurn();
        }

        private async Task NextTurn()
        {
            _lobby.NextTurn();
            await NextTurnInternal();
        }

        public async Task LooseTimerAsync()
        {
            if (_gameState != GameState.Running) // No need to check if we already lost
                return;
            if (_postImage || _checkingAnswer) // If we are already doing something (posting image or checking answer) we wait for it
                return;
            if (_startTime.AddSeconds(_timer).CompareTo(DateTime.Now) < 0)
            {
                if (HaveMultiplayerLobby() && _multiType == APreload.MultiplayerType.BestOf)
                {
                    _bestOfRemainingRounds--;
                    string finalStr = Sentences.TimeOut(_chan.GuildId) + Sentences.CurrentScore(_chan.GuildId) + Environment.NewLine;
                    foreach (var name in _lobby.GetFullNames())
                    {
                        if (_bestOfScore.ContainsKey(name))
                            finalStr += name + ": " + _bestOfScore[name] + Environment.NewLine;
                        else
                            finalStr += name + ": 0" + Environment.NewLine;
                    }
                    await PostText(finalStr);
                    if (_bestOfRemainingRounds == 0)
                    {
                        List<string> bestName = new List<string>();
                        int bestScore = 0;
                        foreach (var name in _lobby.GetFullNames())
                        {
                            int currScore = _bestOfScore.ContainsKey(name) ? _bestOfScore[name] : 0;
                            if (currScore == bestScore)
                                bestName.Add(name);
                            else if (currScore > bestScore)
                            {
                                bestName = new List<string>();
                                bestName.Add(name);
                            }
                        }
                        if (bestName.Count == _lobby.GetFullNames().Count)
                            await PostText(Sentences.Draw(_chan.GuildId));
                        else
                            await PostText(Sentences.WonMulti(_chan.GuildId, string.Join(", ", bestName)));
                        _gameState = GameState.Lost;
                    }
                    else
                    {
                        _bestOfTries = new Dictionary<string, int>();
                        await PostAsync();
                    }
                }
                else
                {
                    await LooseAsync(Sentences.TimeoutGame(_chan.GuildId));
                }
            }
        }

        public bool DidLost()
            => _gameState == GameState.Lost;

        public bool IsWaitingForPlayers()
            => _gameState == GameState.WaitingForPlayers;

        public async Task LooseAsync(string reason)
        {
            _gameState = GameState.Lost;
            if (HaveMultiplayerLobby()) // Multiplayer scores aren't saved
            {
                if (_multiType == APreload.MultiplayerType.Elimination)
                {
                    _lobby.RemoveCurrentPlayer();
                    if (_lobby.HaveEnoughPlayer())
                    {
                        _gameState = GameState.Running;
                        _startTime = DateTime.Now;
                        await PostText(Sentences.YouLost(_chan.GuildId) + (reason == null ? "" : reason + Environment.NewLine) + Sentences.AnnounceTurn(_chan.GuildId, _lobby.GetTurnName()));
                        _postImage = false;
                        return;
                    }
                    await PostText(Sentences.YouLost(_chan.GuildId) + (reason == null ? "" : reason + Environment.NewLine) + await GetLoose() + Environment.NewLine + Sentences.WonMulti(_chan.GuildId, _lobby.GetLastStanding()));
                }
                else
                    throw new ArgumentException("Multiplayer game " + _gameName + " ended in an unexpected way: " + reason);
            }
            else
            {
                if (reason == null)
                    await SaveScores(Sentences.YouLost(_chan.GuildId) + await GetLoose());
                else
                    await SaveScores(Sentences.YouLost(_chan.GuildId) + reason + Environment.NewLine + await GetLoose());
            }
        }

        private async Task SaveScores(string reason)
        {
            var newScore = await Program.p.db.SetNewScore(_saveName, _score, _chan.GuildId, string.Join("|", _contributors));
            if (newScore.Item1 == Db.Db.Comparaison.Best)
                await PostText(reason + Environment.NewLine + Sentences.NewBestScore(_chan.GuildId, newScore.Item2.ToString(), _score.ToString()));
            else if (newScore.Item1 == Db.Db.Comparaison.Equal)
                await PostText(reason + Environment.NewLine + Sentences.EqualizedScore(_chan.GuildId, _score.ToString()));
            else
                await PostText(reason + Environment.NewLine + Sentences.DidntBeatScore(_chan.GuildId, newScore.Item2.ToString(), _score.ToString()));
        }

        private async Task PostFromUrl(string url)
        {
            using (HttpClient hc = new HttpClient())
            {
                if (_isCropped || _isShaded != APreload.Shadow.None) // If image need to be crop we can't just upload it with the stream, we need to download it and modify it before
                {
                    string fileExtension;
                    if (url.Contains(".png")) fileExtension = ".png";
                    else if (url.Contains(".jpg")) fileExtension = ".jpg";
                    else if (url.Contains(".jpeg")) fileExtension = ".jpeg";
                    else if (url.Contains(".gif")) fileExtension = ".gif";
                    else throw new InvalidOperationException("Invalid extension for " + url);
                    string path = _gameName + DateTime.Now.ToString("HHmmss") + Program.p.rand.Next() + fileExtension;

                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(await (await hc.GetAsync(url)).Content.ReadAsStreamAsync());
                    if (_isCropped)
                    {
                        System.Drawing.Bitmap finalBmp = new System.Drawing.Bitmap(bmp.Width / 2, bmp.Height);
                        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalBmp);
                        g.DrawImage(bmp, 0, 0, new System.Drawing.Rectangle(Program.p.rand.Next(0, 2) == 0 ? 0 : bmp.Width / 2, 0, bmp.Width / 2, bmp.Height), System.Drawing.GraphicsUnit.Pixel);
                        bmp = finalBmp;
                    }
                    if (_isShaded != APreload.Shadow.None)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            for (int y = 0; y < bmp.Height; y++)
                            {
                                var pixel = bmp.GetPixel(x, y);
                                if ((_isShaded == APreload.Shadow.Transparency && pixel.A > 0)
                                    || (_isShaded == APreload.Shadow.White && (pixel.R != 255 && pixel.G != 255 && pixel.B != 255)))
                                    bmp.SetPixel(x, y, System.Drawing.Color.White);
                                else
                                    bmp.SetPixel(x, y, System.Drawing.Color.Black);
                            }
                        }
                    }
                    bmp.Save(path);
                    await _chan.SendFileAsync(path);
                    File.Delete(path);
                }
                else
                {
                    Stream s = await hc.GetStreamAsync(url);
                    Stream s2 = await hc.GetStreamAsync(url); // We create a new Stream because the first one is altered.
                    using (MemoryStream ms = new MemoryStream())
                    {
                        await s.CopyToAsync(ms);
                        if (ms.ToArray().Length < 8000000)
                            await _chan.SendFileAsync(s2, "Sanara-image." + url.Split('.').Last());
                        else
                            await _chan.SendMessageAsync(url);
                    }
                }
            }
        }

        protected async Task PostText(string msg)
        {
            await _chan.SendMessageAsync(msg);
        }

        protected async Task PostFromLocalPath(string path)
        {
            await _chan.SendFileAsync(path);
        }

        protected async Task PostText(Func<ulong, string> fct) // Called by children to display sentences
            => await PostText(fct(_chan.GuildId));

        protected string GetStringFromSentence(Func<ulong, string> fct)
            => fct(_chan.GuildId);

        protected ulong GetGuildId() // Use GetStringFromSentence instead if possible
            => _chan.GuildId;

        protected string GetRandomPath() // Add this to a local file to make sure it doesn't already exists
        {
            return _chan.Id + DateTime.Now.ToString("HHmmss") + Program.p.rand.Next(int.MaxValue);
        }

        /// <summary>
        /// How files are send
        /// </summary>
        protected enum PostType
        {
            Url,
            Text,
            LocalPath
        }

        /// <summary>
        /// Current game state
        /// </summary>
        private enum GameState
        {
            WaitingForPlayers,
            Running,
            Lost
        }

        private ITextChannel    _chan; // Channel where the game is
        private List<ulong>     _contributors; // Ids of the users that contributed to the current score
        private string          _saveName; // Name the game will have in the db
        private string          _gameName;
        private int             _score; // Current score
        protected List<string>  _dictionnary; // Game dictionnary
        private bool            _postImage; // True is Sanara is busy posting an image
        private bool            _checkingAnswer; // Used for timer
        private GameState       _gameState; // True if the game is lost
        private DateTime        _startTime; // When the game started
        private int             _timer; // Number of seconds before the player loose
        private MultiplayerLobby _lobby; // Null if game session is solo
        private bool            _isFound; // Fix a bug where 2 users could answer at the same time
        private bool            _isCropped; // Difficulty level, image is cut in half
        private APreload.Shadow _isShaded; // Difficulty level, only display shadow
        private APreload.MultiplayerType _multiType; // How multiplayer is played

        // Multiplayer "BestOf" Game Mode
        private Dictionary<string, int> _bestOfScore;
        private Dictionary<string, int> _bestOfTries;
        private int _bestOfRemainingRounds;
        private const int nbMaxTry = 3;
    }
}
