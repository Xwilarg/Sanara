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
using SanaraV2.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Games
{
    public abstract class AGame
    {
        protected AGame(ITextChannel chan, List<string> dictionnary, Config config)
        {
            _chan = chan;
            if (dictionnary == null) // Dictionnary failed to load
                throw new NoDictionnaryException();
            _dictionnary = dictionnary;
            _contributors = new List<ulong>();
            _saveName = config.gameName + (config.difficulty == Difficulty.Easy ? "-easy" : "") + (config.isFull ? "-full" : "");
            _score = 0;
            _postImage = false;
            _checkingAnswer = false;
            _didLost = false;
            _startTime = DateTime.Now;
            _timer = config.refTime * (int)config.difficulty;
            Init();
        }

        public bool IsSelf(ulong chanId) // Allow to check if a game is running in this channel
            => _chan.Id == chanId;

        public void Cancel()
        {
            _didLost = true;
        }

        protected abstract void Init(); // User must not use the ctor, they must init things in this function (because GetPost is called from this ctor, who is called before the child ctor)
        protected abstract Task<string[]> GetPostAsync();
        protected abstract PostType GetPostType();
        protected abstract Task<string> GetCheckCorrectAsync(string userAnswer); // Return null if suceed, else return error message
        protected abstract Task<string> GetLoose();
        protected abstract bool CongratulateOnGuess(); // Say "Congratulation you found the right answer" on a guess
        protected abstract string Help(); // null is no help

        public async Task PostAsync()
        {
            if (_didLost)
                return;
            _postImage = true;
            int counter = 0;
            while (true)
            {
                if (_didLost) // Can happen if the game is canceled
                    return;
                try
                {
                    if (GetPostType() == PostType.Text)
                        foreach (string s in await GetPostAsync())
                            await PostText(s);
                    else
                        foreach (string s in await GetPostAsync())
                        {
                            if (!Utilities.IsLinkValid(s))
                                throw new ArgumentException("Invalid link " + s);
                            await PostFromUrl(s);
                        }
                    _startTime = DateTime.Now;
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
                        await Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
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
                        await Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
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
            if (_didLost)
                return;
            _checkingAnswer = true;
            if (_postImage)
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
                if (error != "")
                    await PostText(error);
                _checkingAnswer = false;
                return;
            }
            if (!_contributors.Contains(user.Id))
                _contributors.Add(user.Id);
            if (CongratulateOnGuess())
                await PostText(Sentences.GuessGood(_chan.GuildId));
            _score++;
            await PostAsync();
            _checkingAnswer = false;
        }

        public async Task LooseTimerAsync()
        {
            if (_didLost) // No need to check if we already lost
                return;
            if (_postImage || _checkingAnswer) // If we are already doing something (posting image or checking answer) we wait for it
                return;
            if (_startTime.AddSeconds(_timer).CompareTo(DateTime.Now) < 0)
                await LooseAsync(Sentences.TimeoutGame(_chan.GuildId));
        }

        public bool DidLost()
            => _didLost;

        public async Task LooseAsync(string reason)
        {
            _postImage = true; // We make sure that the user isn't able to send things anymore
            if (reason == null)
                await SaveScores(await GetLoose());
            else
                await SaveScores(reason + Environment.NewLine + await GetLoose());
            _didLost = true;
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

        private async Task PostText(string msg)
        {
            await _chan.SendMessageAsync(msg);
        }

        protected async Task PostText(Func<ulong, string> fct) // Called by children to display sentences
            => await PostText(fct(_chan.GuildId));

        protected string GetStringFromSentence(Func<ulong, string> fct)
            => fct(_chan.GuildId);

        protected ulong GetGuildId() // Use GetStringFromSentence instead if possible
            => _chan.GuildId;

        protected enum PostType
        {
            Url,
            Text
        }

        private ITextChannel    _chan; // Channel where the game is
        private List<ulong>     _contributors; // Ids of the users that contributed to the current score
        private string          _saveName; // Name the game will have in the db
        private int             _score; // Current score
        protected List<string>  _dictionnary; // Game dictionnary
        private bool            _postImage; // True is Sanara is busy posting an image
        private bool            _checkingAnswer; // Used for timer
        private bool            _didLost; // True if the game is lost
        private DateTime        _startTime; // When the game started
        private int             _timer; // Number of seconds before the player loose
    }
}
