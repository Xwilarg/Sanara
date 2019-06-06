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

namespace SanaraV2.Games
{
    /// <summary>
    /// Used to manage the different players in case of a multiplayer game
    /// </summary>
    public class MultiplayerLobby
    {
        public MultiplayerLobby(ulong owner)
        {
            players = new List<ulong>();
            players.Add(owner);
            startTime = DateTime.Now;
        }

        public bool AddPlayer(ulong player)
        {
            if (players.Contains(player))
                return false;
            players.Add(player);
            return true;
        }

        public bool IsReady()
            => startTime.AddSeconds(lobbyTime).CompareTo(DateTime.Now) <= 0;

        public bool HaveEnoughPlayer()
            => players.Count > 1;

        private List<ulong> players; // Players in the lobby
        private DateTime startTime; // Time when the game was created (the lobby stay open X seconds so^players can join it)

        private const int lobbyTime = 10; // Seconds the lobby stay open before the game start
    }
}
