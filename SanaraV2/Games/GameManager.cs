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
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SanaraV2.Games
{
    public class GameManager
    {
        public GameManager()
        {
            _games = new List<AGame>();
            _gameThread = new Thread(new ThreadStart(GameLoop));
            _gameThread.Start();
            _gamesTmp = new List<ulong>();
        }

        // Cancel the current game
        public bool Cancel(ulong chanId)
        {
            AGame game = _games.Find(x => x.IsSelf(chanId));
            if (game == null)
                return false;
            game.Cancel();
            return true;
        }

        // If failure return an error message, else return null
        public async Task<Func<ulong, string>> Play(string[] args, ITextChannel chan)
        {
            if (_games.Any(x => x.IsSelf(chan.Id)))
                return Sentences.GameAlreadyRunning;
            _gamesTmp.Add(chan.Id);
            var elem = await PlayInternal(args, chan);
            _gamesTmp.Remove(chan.Id);
            return (elem);
        }

        private async Task<Func<ulong, string>> PlayInternal(string[] args, ITextChannel chan)
        {
            if (args.Length == 0)
                return Sentences.InvalidGameName;
            string gameName = args[0];
            Difficulty difficulty = Difficulty.Normal;
            bool isFull = false;
            if (args.Length > 1)
            {
                foreach (string s in args.Skip(1).Select(x => x.ToLower()))
                {
                    switch (s)
                    {
                        case "full":
                            isFull = true;
                            break;

                        case "easy":
                            difficulty = Difficulty.Easy;
                            break;

                        case "normal":
                            difficulty = Difficulty.Normal;
                            break;

                        default:
                            return Sentences.InvalidDifficulty;
                    }
                }
            }
            foreach (var game in Constants.allGames)
            {
                APreload preload = (APreload)Activator.CreateInstance(game.Item1);
                if (preload.ContainsName(gameName))
                {
                    if (!chan.IsNsfw && preload.IsNsfw())
                        return Modules.Base.Sentences.ChanIsNotNsfw;
                    if (isFull && !preload.DoesAllowFull())
                        return Sentences.FullNotAvailable;
                    try
                    {
                        await chan.SendMessageAsync(preload.GetRules(chan.GuildId) + Environment.NewLine +
                            Sentences.RulesTimer(chan.GuildId, preload.GetTimer() * (int)difficulty) + Environment.NewLine +
                            Sentences.RulesReset(chan.GuildId));
                        AGame newGame = (AGame)Activator.CreateInstance(game.Item2, chan, new Config(preload.GetTimer(), difficulty, preload.GetGameName(), isFull));
                         _games.Add(newGame);
                        await newGame.PostAsync();
                        if (Program.p.sendStats)
                            await Program.p.UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("games", preload.GetGameName()) });
                        return null;
                    }
                    catch (NoDictionnaryException)
                    {
                        return Sentences.NoDictionnary;
                    }
                }
            }
            return Sentences.InvalidGameName;
        }

        public async Task ReceiveMessageAsync(string message, SocketUser user, ulong chanId) // Called everytimes a message is sent somewhere
        {
            AGame game = _games.Find(x => x.IsSelf(chanId));
            if (game != null)
            game.CheckCorrectAsync(user, message).GetAwaiter().GetResult();
        }

        private void GameLoop()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                for (int i = _games.Count - 1; i >= 0; i--)
                {
                    _games[i].LooseTimerAsync().GetAwaiter().GetResult();
                }
                _games.RemoveAll(x => x.DidLost());
                Thread.Sleep(250);
            }
        }

        private List<AGame> _games;
        private Thread _gameThread;
        private List<ulong> _gamesTmp; // To be sure that two players don't start a game at the same time in the same chan
    }
}
