using Discord;

namespace SanaraV2.Community
{
    public class Profile
    {
        /// <summary>
        /// Create empty profile
        /// </summary>
        public Profile(IUser user)
        {
            _visibility = Visibility.FriendsOnly;
            _username = user.ToString();
        }

        private Visibility _visibility;
        private string _username;
    }
}
