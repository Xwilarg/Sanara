using System;
using System.Collections.Generic;
using System.Globalization;

namespace SanaraV2.Community
{
    public static class AchievementList
    {
        public static Achievement GetAchievement(AchievementID id)
        {
            return _achievements[id];
        }

        private static Dictionary<AchievementID, Achievement> _achievements = new Dictionary<AchievementID, Achievement>
        {
            { AchievementID.SendCommands, new Achievement(100, 1000, 10000) },
            { AchievementID.ThrowErrors, new Achievement(100, 1000, 10000) },
            { AchievementID.GoodScores, new Achievement(100, 1000, 10000, ChangeValueIfBetter) },
            { AchievementID.GoodScoresShadow, new Achievement(100, 1000, 10000, ChangeValueIfBetter) },
            { AchievementID.PlayWithFriends, new Achievement(100, 1000, 10000, ChangeValueIfBetter) },
            { AchievementID.CommandsDaysInRow, new Achievement(100, 1000, 10000, (value, addData, progression, list) => {
                if (list.Count == 0) // First day something is sent to Sanara
                {
                    list.Add(DateTime.UtcNow.ToString("yyMMddHHmmss"));
                    return 1;
                }
                var dt = DateTime.ParseExact(list[0], "yyMMddHHmmss", CultureInfo.InvariantCulture);
                if (DateTime.UtcNow.Subtract(dt).TotalDays > 1) // Didn't contact Sanara since more than one day
                {
                    list.Clear();
                    list.Add(DateTime.UtcNow.ToString("yyMMddHHmmss"));
                    return 1;
                }
                return progression + 1; // One more day since last contact
            })
            }
        };

        private static int ChangeValueIfBetter(int value, string _, int progression, List<string> __)
            => value > progression ? value : progression;
    }
}
