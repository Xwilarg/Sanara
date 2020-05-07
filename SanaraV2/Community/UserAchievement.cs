namespace SanaraV2.Community
{
    public class UserAchievement
    {
        public UserAchievement(Achievement achievement, int progression)
        {
            _achievement = achievement;
            _progression = progression;
        }

        public override string ToString()
        {
            return _progression.ToString();
        }

        private Achievement _achievement;
        private int _progression;
    }
}
