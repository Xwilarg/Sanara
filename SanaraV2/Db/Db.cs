/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.
using Newtonsoft.Json;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using SanaraV2.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2.Db
{
    public class Db
    {
        public Db()
        {
            R = RethinkDB.R;
            Languages = new Dictionary<ulong, string>();
            Prefixs = new Dictionary<ulong, string>();
            Availability = new Dictionary<ulong, string>();
        }

        public async Task InitAsync(string dbName = "Sanara")
        {
            this.dbName = dbName;
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                R.DbCreate(dbName).Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Users").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Users").Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Guilds").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Guilds").Run(conn);
        }

        private static readonly string defaultAvailability = "111111111111111";

        public async Task ResetGuild(ulong guildId)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                   .With("Prefix", "s.")
                   .With("Language", "en")
                   .With("Availability", defaultAvailability)
                   ).RunAsync(conn);
        }

        public async Task InitGuild(ulong guildId)
        {
            string guildIdStr = guildId.ToString();
            if (await R.Db(dbName).Table("Guilds").GetAll(guildIdStr).Count().Eq(0).RunAsync<bool>(conn))
            {
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildIdStr)
                    .With("Prefix", "s.")
                    .With("Language", "en")
                    .With("Availability", defaultAvailability)
                    ).RunAsync(conn);
            }
            dynamic json = await R.Db(dbName).Table("Guilds").Get(guildIdStr).RunAsync(conn);
            Languages.Add(guildId, (string)json.Language);
            Prefixs.Add(guildId, (string)json.Prefix);
            string availability = (string)json.Availability;
            if (availability == null)
                Availability.Add(guildId, defaultAvailability);
            else
            {
                string newAvailability = availability;
                while (newAvailability.Length < defaultAvailability.Length)
                    newAvailability += "1";
                Availability.Add(guildId, newAvailability);
            }
        }

        public async Task SetPrefix(ulong guildId, string prefix)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("Prefix", prefix)
                ).RunAsync(conn);
            Prefixs[guildId] = prefix;
        }

        public async Task SetLanguage(ulong guildId, string language)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("Language", language)
                ).RunAsync(conn);
            Languages[guildId] = language;
        }

        public async Task SetAvailability(ulong guildId, Program.Module module, int enable)
        {
            StringBuilder availability = new StringBuilder(Availability[guildId]);
            availability[(int)module] = (char)(enable + '0');
            string res = availability.ToString();
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("Availability", res)
                ).RunAsync(conn);
            Availability[guildId] = res;
        }

        public bool IsAvailable(ulong guildId, Program.Module module)
        {
            return (Availability[guildId][(int)module] == '1');
        }

        public bool AreAllAvailable(ulong guildId)
            => Availability[guildId].All(x => x == '1');

        public bool AreNoneAvailable(ulong guildId)
            => Availability[guildId].Count(x => x == '0') == 2;

        public async Task<string> GetGuild(ulong guildId)
        {
            return (JsonConvert.SerializeObject(await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn)));
        }

        public enum Comparaison
        {
            Best,
            Equal,
            Inferior
        }

        public async Task<Tuple<Comparaison, int>> SetNewScore(string gameName, int score, ulong guildId, string ids)
        {
            string scoreStr = ((string)(await R.Db(dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(conn))[gameName])?.Split('|').First();
            int? currScore = null;
            if (scoreStr != null)
                currScore = int.Parse(scoreStr);
            Comparaison cmp;
            if ((currScore == null && score == 0) || (currScore != null && currScore == score))
                cmp = Comparaison.Equal;
            else if (currScore == null || currScore < score)
                cmp = Comparaison.Best;
            else
                cmp = Comparaison.Inferior;
            if (cmp == Comparaison.Best)
            {
                await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With(gameName, score + "|" + ids)
                ).RunAsync(conn);
            }
            if (currScore == null)
                currScore = 0;
            return (new Tuple<Comparaison, int>(cmp, currScore.Value));
        }

        public async Task<Dictionary<string, Dictionary<string, string>>> GetAllScores()
        {
            Dictionary<string, Dictionary<string, string>> allScores = new Dictionary<string, Dictionary<string, string>>();
            var json = await R.Db(dbName).Table("Guilds").RunAsync(conn);
            foreach (var elem in json)
            {
                Dictionary<string, string> currDict = new Dictionary<string, string>();
                foreach (var game in Constants.allRankedGames)
                {
                    APreload preload = (APreload)Activator.CreateInstance(game.Item1);
                    string gameName = preload.GetGameName();
                    if (elem[gameName] != null) currDict.Add(gameName, elem[gameName].ToString());
                }
                if (currDict.Count > 0)
                    allScores.Add(elem.id.ToString(), currDict);
            }
            return (allScores);
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;

        public Dictionary<ulong, string> Languages { private set; get; }
        public Dictionary<ulong, string> Prefixs { private set; get; }
        public Dictionary<ulong, string> Availability { private set; get; }
    }
}
