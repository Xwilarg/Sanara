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

        public override string ToString()
        {
            return _progression.ToString() + "," + string.Join(",", _additionalDatas);
        }

        public void AddProgression(int value, string addData = null)
        {
            if (addData == null || !_additionalDatas.Contains(addData))
            {
                if (addData != null)
                    _additionalDatas.Add(addData);
                _progression += value;
            }
        }

        private Achievement _achievement;
        private int _progression;
        private List<string> _additionalDatas;
    }
}
