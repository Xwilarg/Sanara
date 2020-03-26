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

        public string JoinGame(ulong guildId, ulong chanId, ulong playerId)
        {
            AGame game = _games.Find(x => x.IsSelf(chanId));
            if (game == null)
                return Sentences.LobbyNoWaiting(guildId);
            if (!game.IsWaitingForPlayers())
                return Sentences.LobbyNoWaiting(guildId);
            if (!game.HaveMultiplayerLobby())
                return Sentences.LobbySoloJoin(guildId);
            if (game.IsFull())
                return Sentences.LobbyFull(guildId);
            if (game.IsPlayerInLobby(playerId))
                return Sentences.LobbyAlreadyInThis(guildId);
            if (_games.Any(x => x.IsPlayerInLobby(playerId)))
                return Sentences.LobbyAlreadyIn(guildId);
            game.AddPlayerToLobby(playerId);
            return Sentences.LobbyJoined(guildId, game.GetName());
        }

        public string LeaveGame(ulong guildId, ulong chanId, ulong playerId)
        {
            AGame game = _games.Find(x => x.IsSelf(chanId));
            if (game == null)
                return Sentences.LobbyNoWaiting(guildId);
            if (!game.IsWaitingForPlayers())
                return Sentences.LobbyAlreadyStarted(guildId);
            if (!game.HaveMultiplayerLobby())
                return Sentences.LobbySoloLeave(guildId);
            if (!game.IsPlayerInLobby(playerId))
                return Sentences.LobbyAlreadyOut(guildId);
            game.RemovePlayerFromLobby(playerId);
            return Sentences.LobbyLeaved(guildId) + (game.IsLobbyEmpty() ? Environment.NewLine + Sentences.LobbyEmpty(guildId) : "");
        }

        public async Task<string> StartGame(ulong guildId, ulong chanId, ulong playerId)
        {
            AGame game = _games.Find(x => x.IsSelf(chanId));
            if (game == null)
                return Sentences.LobbyNoWaiting(guildId);
            if (!game.IsWaitingForPlayers())
                return Sentences.LobbyNoWaiting(guildId);
            if (!game.IsPlayerInLobby(playerId))
                return Sentences.LobbyAlreadyOut(guildId);
            if (!game.HaveEnoughPlayer())
                return Sentences.LobbyNotEnoughPlayer(guildId);
            await game.Start();
            return null;
        }

        // If failure return an error message, else return null
        // The error message is a callback to the function to display it
        public async Task<Func<ulong, string>> Play(string[] args, ITextChannel chan, ulong playerId)
        {
            if (_games.Any(x => x.IsSelf(chan.Id)))
                return Sentences.GameAlreadyRunning;
            _gamesTmp.Add(chan.Id);
            var elem = await PlayInternal(args, chan, playerId);
            _gamesTmp.Remove(chan.Id);
            return (elem);
        }

        private async Task<Func<ulong, string>> PlayInternal(string[] args, ITextChannel chan, ulong playerId)
        {
            if (args.Length == 0)
                return Sentences.InvalidGameName;
            string gameName = args[0].ToLower();
            Difficulty difficulty = Difficulty.Normal;
            bool isFull = false;
            bool sendImage = false;
            bool isCropped = false;
            APreload.Shadow isShaded = APreload.Shadow.None;
            APreload.Multiplayer isMultiplayer = APreload.Multiplayer.SoloOnly;
            if (args.Length > 1)
            {
                foreach (string s in args.Skip(1).Select(x => x.ToLower()))
                {
                    switch (s)
                    {
                        case "full":
                            isFull = true;
                            break;

                        case "multi":
                        case "multiplayer":
                            isMultiplayer = APreload.Multiplayer.MultiOnly;
                            break;

                        case "easy":
                            difficulty = Difficulty.Easy;
                            break;

                        case "normal":
                        case "solo":
                            break; // These case exist so the user can precise them, but they do nothing

                        case "crop":
                        case "cropped":
                            isCropped = true;
                            break;

                        case "shade":
                        case "shadow":
                        case "shaded":
                            isShaded = APreload.Shadow.Transparency;
                            break;

                        case "image":
                            sendImage = true;
                            break;

                        default:
                            return Sentences.InvalidGameArgument;
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
                    if (isMultiplayer == APreload.Multiplayer.MultiOnly && preload.DoesAllowMultiplayer() == APreload.Multiplayer.SoloOnly)
                        return Sentences.MultiNotAvailable;
                    if (isMultiplayer == APreload.Multiplayer.SoloOnly && preload.DoesAllowMultiplayer() == APreload.Multiplayer.MultiOnly)
                        return Sentences.SoloNotAvailable;
                    if (isFull && !preload.DoesAllowFull())
                        return Sentences.FullNotAvailable;
                    if (sendImage && !preload.DoesAllowSendImage())
                        return Sentences.SendImageNotAvailable;
                    if (isCropped && !preload.DoesAllowCropped())
                        return Sentences.CropNotAvailable;
                    if (isShaded != APreload.Shadow.None && preload.DoesAllowShadow() == APreload.Shadow.None)
                        return Sentences.ShadowNotAvailable;
                    if (isShaded != APreload.Shadow.None)
                        isShaded = preload.DoesAllowShadow();
                    try
                    {
                        string introMsg = "";
                        if (isMultiplayer == APreload.Multiplayer.MultiOnly)
                        {
                            introMsg += Sentences.LobbyCreation(chan.GuildId, MultiplayerLobby.lobbyTime.ToString()) + Environment.NewLine + Environment.NewLine;
                        }
                        introMsg += "**" + Sentences.Rules(chan.GuildId) + ":**" + Environment.NewLine +
                            preload.GetRules(chan.GuildId, isMultiplayer == APreload.Multiplayer.MultiOnly) + Environment.NewLine;
                        if (isMultiplayer == APreload.Multiplayer.MultiOnly)
                        {
                            if (preload.GetMultiplayerType() == APreload.MultiplayerType.Elimination)
                                introMsg += Sentences.RulesMultiElimination(chan.GuildId) + Environment.NewLine;
                            else
                                introMsg += Sentences.RulesMultiBestOf(chan.GuildId, AGame.nbMaxTry, AGame.nbQuestions) + Environment.NewLine;
                        }
                        introMsg += Sentences.RulesTimer(chan.GuildId, preload.GetTimer() * (int)difficulty) + Environment.NewLine + Environment.NewLine +
                            Sentences.RulesReset(chan.GuildId);
                        await chan.SendMessageAsync(introMsg);
                        AGame newGame = (AGame)Activator.CreateInstance(game.Item2, chan, new Config(preload.GetTimer(), difficulty, preload.GetGameName(), isFull, sendImage, isCropped, isShaded, isMultiplayer, preload.GetMultiplayerType()), playerId);
                         _games.Add(newGame);
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
                await game.CheckCorrectAsync(user, message);
        }

        private void GameLoop()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                for (int i = _games.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        AGame g = _games[i];
                        if (g.IsWaitingForPlayers())
                        {
                            if (g.IsReady())
                            {
                                if (g.HaveEnoughPlayer())
                                    g.Start().GetAwaiter().GetResult();
                                else
                                    g.DisplayCantStart().GetAwaiter().GetResult();
                            }
                        }
                        else
                            _games[i].LooseTimerAsync().GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                    }
                    _games.RemoveAll(x => x.DidLost());
                    Thread.Sleep(250);
                }
            }
        }

        private List<AGame> _games;
        private Thread _gameThread;
        private List<ulong> _gamesTmp; // To be sure that two players don't start a game at the same time in the same chan
    }
}
