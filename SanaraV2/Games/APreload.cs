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
using System.Linq;

namespace SanaraV2.Games
{
    public abstract class APreload
    {
        protected APreload(string[] names, int timer, Func<ulong, string> gameSentence)
        {
            _names = names;
            _timer = timer;
            _gameSentence = gameSentence;
        }

        public abstract bool IsNsfw();
        public abstract bool DoesAllowFull(); // Allow 'full' attribute
        public abstract bool DoesAllowCropped(); // Allow 'crop" attribute
        public abstract bool DoesAllowShadow(); // Allow 'shadow" attribute
        public abstract Multiplayer DoesAllowMultiplayer(); // Allow 'multi' attribute
        public enum Multiplayer
        {
            SoloOnly,
            MultiOnly,
            Both
        }
        public abstract string GetRules(ulong guildId, bool isMultiplayer);

        public bool ContainsName(string name)
            => _names.Contains(name);

        public int GetTimer()
            => _timer;

        public string GetGameName()
            => _names[0];

        public string GetGameSentence(ulong guildId)
            => _gameSentence(guildId);

        private string[] _names;
        private int _timer;
        private Func<ulong, string> _gameSentence;
    }
}
