using System.Collections.Generic;

namespace SanaraV2.Community
{
    public static class AchievementList
    {
        public static Achievement GetAchievement(int id)
        {
            return _achievements[id];
        }

        private static Dictionary<int, Achievement> _achievements = new Dictionary<int, Achievement>
        {
            { 0, new Achievement() }
        };
    }
}
