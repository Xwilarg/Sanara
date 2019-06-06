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

using System;
using System.Collections.Generic;
using System.Linq;

namespace SanaraV2.Games
{
    /// <summary>
    /// Used to manage the different players in case of a multiplayer game
    /// </summary>
    public class MultiplayerLobby
    {
        public MultiplayerLobby(ulong owner)
        {
            _players = new List<ulong>();
            _players.Add(owner);
            _startTime = DateTime.Now;
        }

        public void AddPlayer(ulong player)
            => _players.Add(player);

        public bool RemovePlayer(ulong player)
            => _players.Remove(player);

        public bool IsPlayerIn(ulong player)
            => _players.Contains(player);

        public bool IsLobbyEmpty()
            => _players.Count == 0;

        public bool IsReady()
            => _startTime.AddSeconds(lobbyTime).CompareTo(DateTime.Now) <= 0;

        public bool HaveEnoughPlayer()
            => _players.Count > 1;

        public string GetReadyMessage(ulong guildId)
            => Sentences.Participants(guildId) + ":" + Environment.NewLine + string.Join(", ", _players.Select(x => "<@" + x + ">"));

        private List<ulong> _players; // Players in the lobby
        private DateTime    _startTime; // Time when the game was created (the lobby stay open X seconds so^players can join it)

        public static readonly int lobbyTime = 10; // Seconds the lobby stay open before the game start
    }
}
