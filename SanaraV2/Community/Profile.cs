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
using System.Threading.Tasks;

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
            _creationDate = DateTime.UtcNow;
            _backgroundColor = System.Drawing.Color.White;
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
            var colorString = token["BackgroundColor"].Value<string>().Split(',');
            _backgroundColor = System.Drawing.Color.FromArgb(int.Parse(colorString[0]), int.Parse(colorString[1]), int.Parse(colorString[2]));
        }

        public MapObject GetProfileToDb(RethinkDB r)
        {
            return r.HashMap("id", _id)
                    .With("Visibility", (int)_visibility)
                    .With("Username", _username)
                    .With("Friends", string.Join(",", _friends))
                    .With("Description", _description)
                    .With("Achievements", string.Join("|", _achievements.Select(x => x.Key + "," + x.Value.ToString())))
                    .With("CreationDate", _creationDate.ToString("yyMMddHHmmss"))
                    .With("BackgroundColor", _backgroundColor.R + "," + _backgroundColor.G + "," + _backgroundColor.B);
        }

        public System.Drawing.Image GetProfilePicture(IUser user)
        {
            using (HttpClient hc = new HttpClient())
                return System.Drawing.Image.FromStream(hc.GetStreamAsync(user.GetAvatarUrl(ImageFormat.Png)).GetAwaiter().GetResult());
        }

        public void UpdateProfile(IUser user)
        {
            _username = user.ToString();
            Program.p.db.UpdateProfile(this);
        }

        public bool UpdateDescription(string description)
        {
            if (description.Length > 400)
                return false;
            description = description.Replace("\\n", "\n");
            string tmp = "";
            while (description.Length > 40)
            {
                tmp += description.Substring(0, 40) + "\n";
                description = description.Substring(40);
            }
            tmp += description;
            if (tmp.Count(x => x == '\n') > 9) // More than 10 lines
                return false;
            _description = tmp;
            Program.p.db.UpdateProfile(this);
            return true;
        }

        public async Task<bool> UpdateColor(string[] color)
        {
            var result = await Features.Tools.Code.SearchColor(color, Program.p.rand);
            if (result.error == Features.Tools.Error.Image.None)
            {
                var c = result.answer.discordColor;
                _backgroundColor = System.Drawing.Color.FromArgb(c.R, c.G, c.B);
                Program.p.db.UpdateProfile(this);
                return true;
            }
            return false;
        }

        public string GetUsername()
            => _username;

        public string GetDescription()
            => _description;

        public int GetFriendsCount()
            => _friends.Count;

        public System.Drawing.Color GetBackgroundColor()
            => _backgroundColor;

        private Visibility _visibility;
        private string _username;
        private List<ulong> _friends;
        private string _description;
        private Dictionary<int, UserAchievement> _achievements;
        private DateTime _creationDate;
        private System.Drawing.Color _backgroundColor;
        
        private string _id;
    }
}
