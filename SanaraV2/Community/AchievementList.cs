using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SanaraV2.Community
{
    public static class AchievementList
    {
        public static Achievement GetAchievement(AchievementID id)
        {
            return _achievements[id];
        }

        public static Stream GetAchievementStream(string path)
        {
            if (!_allAchievementsFiles.ContainsKey(path))
            {
                Stream s = new MemoryStream(File.ReadAllBytes(path));
                _allAchievementsFiles.Add(path, s);
                return s;
            }
            return _allAchievementsFiles[path];
        }

        private static Dictionary<AchievementID, Achievement> _achievements = new Dictionary<AchievementID, Achievement>
        {
            { AchievementID.SendCommands, new Achievement(100, 1000, 10000, "AchievementCommands.png") },
            { AchievementID.ThrowErrors, new Achievement(1, 10, 50, "AchievementBug.png") },
            { AchievementID.GoodScores, new Achievement(10, 25, 50, "AchievementGames.png", ChangeValueIfBetter) },
            { AchievementID.GoodScoresShadow, new Achievement(5, 20, 40, "AchievementShadowGame.png", ChangeValueIfBetter) },
            { AchievementID.PlayWithFriends, new Achievement(2, 5, 10, "AchievementFriends.png", ChangeValueIfBetter) },
            { AchievementID.CommandsDaysInRow, new Achievement(7, 14, 30, "AchievementEveryDays.png", (value, addData, progression, list) => {
                if (list.Count == 0) // First day something is sent to Sanara
                {
                    list.Add(DateTime.UtcNow.ToString("yyMMdd"));
                    return 1;
                }
                var dt = DateTime.ParseExact(list[0], "yyMMdd", CultureInfo.InvariantCulture);
                if (DateTime.UtcNow.Subtract(dt).TotalDays > 2) // Didn't contact Sanara since more than one day
                {
                    list.Clear();
                    list.Add(DateTime.UtcNow.ToString("yyMMdd"));
                    return 1;
                }
                if (DateTime.UtcNow.Subtract(dt).TotalDays > 1)
                    return progression + 1; // One more day since last contact
                return progression;
            })
            },
            { AchievementID.AddFriends, new Achievement(5, 10, 25, "AchievementProfileFriends.png") },
            { AchievementID.ShareAchievement, new Achievement(0, 5, 20, "AchievementShare.png") },
            { AchievementID.DoDifferentsBoorus, new Achievement(10, 50, 100, "AchievementImage.png") },
            { AchievementID.DoDifferentsCommands, new Achievement(10, 25, 50, "AchievementDifferentsCommands.png") },
        };

        private static Dictionary<string, Stream> _allAchievementsFiles = new Dictionary<string, Stream>();

        private static int ChangeValueIfBetter(int value, string _, int progression, List<string> __)
            => value > progression ? value : progression;
    }
}
