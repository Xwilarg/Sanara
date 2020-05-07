using Discord;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Model;
using System.Collections.Generic;
using System.Linq;

namespace SanaraV2.Community
{
    public class Profile
    {
        /// <summary>
        /// Create empty profile
        /// </summary>
        public Profile(IUser user)
        {
            _id = user.Id.ToString();

            _visibility = Visibility.FriendsOnly;
            _username = user.ToString();
            _friends = new List<ulong>();
            _description = "";
            _achievements = new Dictionary<int, UserAchievement>();
        }

        /// <summary>
        /// Create profile from db
        /// </summary>
        /// <param name="json"></param>
        public Profile(string id, JToken token)
        {
            _id = id;

            _visibility = (Visibility)token["Visibility"].Value<int>();
            _username = token["Username"].Value<string>();
            _friends = token["Friends"].Value<string>().Split(',').Select(x => ulong.Parse(x)).ToList();
            _description = token["Description"].Value<string>();
            _achievements = token["Achievements"].Value<string>().Split('|').Select((x) =>
            {
                var split = x.Split(',');
                int a_id = int.Parse(split[0]);
                return new KeyValuePair<int, UserAchievement>(a_id, new UserAchievement(AchievementList.GetAchievement(a_id), int.Parse(split[1])));
            }).ToDictionary(x => x.Key, x => x.Value);
        }

        public MapObject GetProfileToDb(RethinkDB r)
        {
            return r.HashMap("id", _id)
                    .With("Visibility", (int)_visibility)
                    .With("Username", _username)
                    .With("Friends", string.Join(",", _friends))
                    .With("Description", _description)
                    .With("Achievements", string.Join("|", _achievements.Select(x => x.Key + "," + x.Value.ToString())));
        }

        private Visibility _visibility;
        private string _username;
        private List<ulong> _friends;
        private string _description;
        private Dictionary<int, UserAchievement> _achievements;
        
        private string _id;
    }
}
