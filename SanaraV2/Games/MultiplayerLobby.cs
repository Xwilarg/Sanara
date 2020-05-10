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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
            _names = new List<string>();
            _fullNames = new List<string>();
            _startTime = DateTime.Now;
        }

        public void AddPlayer(ulong player)
            => _players.Add(player);

        public bool RemovePlayer(ulong player)
            => _players.Remove(player);

        public void RemoveCurrentPlayer()
        {
            _players.RemoveAt(_currTurn);
            _names.RemoveAt(_currTurn);
            if (_currTurn == _players.Count)
                _currTurn = 0;
        }

        public bool IsPlayerIn(ulong player)
            => _players.Contains(player);

        public bool IsLobbyEmpty()
            => _players.Count == 0;

        public bool IsReady()
            => _startTime.AddSeconds(lobbyTime).CompareTo(DateTime.Now) <= 0;

        public bool HaveEnoughPlayer()
            => _players.Count > 1;

        public int GetNumberPlayers()
            => _players.Count;

        public string GetLastStanding() // Name of last standing player, called at the end of the game to know the winner
            => _names[0];

        public string GetReadyMessage(ulong guildId)
            => Sentences.Participants(guildId) + Environment.NewLine + string.Join(", ", _players.Select(x => "<@" + x + ">"));

        public void NextTurn()
        {
            _currTurn++;
            if (_currTurn == _players.Count)
                _currTurn = 0;
        }

        public async Task<bool> LoadNames(ITextChannel chan)
        {
            _currTurn = 0;
            List<ulong> newPlayers = new List<ulong>();
            while (_players.Count > 0)
            {
                int randomPlayer = Program.p.rand.Next(0, _players.Count);
                newPlayers.Add(_players[randomPlayer]);
                _players.RemoveAt(randomPlayer);
            }
            _players = newPlayers;
            foreach (ulong id in _players)
            {
                IGuildUser user = await chan.GetUserAsync(id);
                if (user == null)
                    return false;
                _names.Add(user.Nickname ?? user.Username);
                _fullNames.Add(user.ToString());
            }
            _allNames = new List<string>(_names).ToArray();
            return true;
        }

        public bool IsMyTurn(ulong player)
            => _players[_currTurn] == player;

        public string GetTurnName()
            => _names[_currTurn];

        public string GetName(int index)
            => _allNames[index];

        public ReadOnlyCollection<string> GetFullNames()
            => _fullNames.AsReadOnly();

        public ReadOnlyCollection<ulong> GetPlayersId()
            => _players.AsReadOnly();

        private List<ulong>     _players; // Players in the lobby
        private List<string>    _names;
        private List<string>    _fullNames; // Names in format xxxxx#1234
        private string[]        _allNames; // Isn't modified when a player loose
        private DateTime        _startTime; // Time when the game was created (the lobby stay open X seconds so^players can join it)
        private int             _currTurn; // Keep track of which turn is it

        public static readonly int lobbyTime = 15; // Seconds the lobby stay open before the game start
    }
}
