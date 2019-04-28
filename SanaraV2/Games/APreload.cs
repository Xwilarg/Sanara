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
    public abstract class APreload
    {
        public APreload(List<string> dictionnary, string[] names, int timer, Func<ulong, string> gameSentence)
        {
            _dictionnary = dictionnary;
            _names = names;
            _timer = timer;
            _gameSentence = gameSentence;
        }

        public abstract bool IsNsfw();
        public abstract bool DoesAllowFull(); // Allow 'full' attribute
        public abstract string GetRules(ulong guildId);

        public List<string> GetDictionnary()
            => _dictionnary;

        public bool ContainsName(string name)
            => _names.Contains(name);

        public int GetTimer()
            => _timer;

        public string GetGameName()
            => _names[0];

        public string GetGameSentence(ulong guildId)
            => _gameSentence(guildId);

        private List<string> _dictionnary;
        private string[] _names;
        private int _timer;
        private Func<ulong, string> _gameSentence;
    }
}
