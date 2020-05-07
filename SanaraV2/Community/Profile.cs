using Discord;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;

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

            using (HttpClient hc = new HttpClient())
                File.WriteAllBytes("Saves/Profiles/Pictures/" + user.Id + ".png", hc.GetByteArrayAsync(user.GetAvatarUrl(ImageFormat.Png, 64)).GetAwaiter().GetResult());
            _visibility = Visibility.FriendsOnly;
            _username = user.ToString();
            _friends = new List<ulong>();
            _description = "";
            _achievements = new Dictionary<int, UserAchievement>();
            _creationDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Create profile from db
        /// </summary>
        /// <param name="json"></param>
        public Profile(string id, JObject token)
        {
            _id = id;

            _visibility = (Visibility)token["Visibility"].Value<int>();
            _username = token["Username"].Value<string>();
            _friends = token["Friends"].Value<string>().Contains(',') ? token["Friends"].Value<string>().Split(',').Select(x => ulong.Parse(x)).ToList() : new List<ulong>();
            _description = token["Description"].Value<string>();
            _achievements = token["Achievements"].Value<string>().Contains('|') ? token["Achievements"].Value<string>().Split('|').Select((x) =>
            {
                var split = x.Split(',');
                int a_id = int.Parse(split[0]);
                return new KeyValuePair<int, UserAchievement>(a_id, new UserAchievement(AchievementList.GetAchievement(a_id), int.Parse(split[1])));
            }).ToDictionary(x => x.Key, x => x.Value) : new Dictionary<int, UserAchievement>();
            _creationDate = DateTime.ParseExact(token["CreationDate"].Value<string>(), "yyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        public MapObject GetProfileToDb(RethinkDB r)
        {
            return r.HashMap("id", _id)
                    .With("Visibility", (int)_visibility)
                    .With("Username", _username)
                    .With("Friends", string.Join(",", _friends))
                    .With("Description", _description)
                    .With("Achievements", string.Join("|", _achievements.Select(x => x.Key + "," + x.Value.ToString())))
                    .With("CreationDate", _creationDate.ToString("yyMMddHHmmss"));
        }

        public System.Drawing.Image GetProfilePicture()
            => System.Drawing.Image.FromFile("Saves/Profiles/Pictures/" + _id + ".png");

        public void UpdateProfile(IUser user)
        {
            _username = user.ToString();
            using (HttpClient hc = new HttpClient())
                File.WriteAllBytes("Saves/Profiles/Pictures/" + user.Id + ".png", hc.GetByteArrayAsync(user.GetAvatarUrl(ImageFormat.Png, 64)).GetAwaiter().GetResult());
            Program.p.db.UpdateProfile(this);
        }

        private Visibility _visibility;
        private string _username;
        private List<ulong> _friends;
        private string _description;
        private Dictionary<int, UserAchievement> _achievements;
        private DateTime _creationDate;
        
        private string _id;
    }
}
