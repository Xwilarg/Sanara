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
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task InitGuild(ulong guildId)
        {
            string guildIdStr = guildId.ToString();
            if (await R.Db(dbName).Table("Guilds").GetAll(guildIdStr).Count().Eq(0).RunAsync<bool>(conn))
            {
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildIdStr)
                    .With("Prefix", "s.")
                    .With("Language", "en")
                    ).RunAsync(conn);
            }
            dynamic json = await R.Db(dbName).Table("Guilds").Get(guildIdStr).RunAsync(conn);
            Languages.Add(guildId, (string)json.Language);
            Prefixs.Add(guildId, (string)json.Prefix);
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

        private RethinkDB R;
        private Connection conn;
        private string dbName;

        public Dictionary<ulong, string> Languages { private set; get; }
        public Dictionary<ulong, string> Prefixs { private set; get; }
    }
}
