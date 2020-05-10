using System;
using System.Collections.Generic;

namespace SanaraV2.Community
{
    public class Achievement
    {
        public Achievement(int lvl1, int lvl2, int lvl3, Func<int, string, int, List<string>, int> specialCallback = null)
        {
            _level1 = lvl1;
            _level2 = lvl2;
            _level3 = lvl3;
            _specialCallback = specialCallback;
        }

        public int GetLevel(int progression)
        {
            if (progression < _level1) return 0;
            if (progression < _level2) return 1;
            if (progression < _level3) return 2;
            return 3;
        }

        public Func<int, string, int, List<string>, int> GetSpecialCallback()
            => _specialCallback;

        private int _level1, _level2, _level3;
        private Func<int, string, int, List<string>, int> _specialCallback;
    }
}
