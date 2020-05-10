using System.Collections.Generic;

namespace SanaraV2.Community
{
    public class UserAchievement
    {
        public UserAchievement(Achievement achievement, int progression, List<string> additionalDatas)
        {
            _achievement = achievement;
            _progression = progression;
            _additionalDatas = additionalDatas;
        }

        public string ToString(bool censorAchievements)
        {
            return (censorAchievements ? "X" : _progression.ToString()) + "," + string.Join(",", _additionalDatas);
        }

        public void AddProgression(int value, string addData)
        {
            var callback = _achievement.GetSpecialCallback();
            if (callback != null)
                _progression = callback(value, addData, _progression, _additionalDatas);
            else // By default achievements just add the given parameter to the progress
            {
                if (addData == null || !_additionalDatas.Contains(addData))
                {
                    if (addData != null)
                        _additionalDatas.Add(addData);
                    _progression += value;
                }
            }
        }

        public int GetLevel()
            => _progression;

        private Achievement _achievement;
        private int _progression;
        private List<string> _additionalDatas;
    }
}
