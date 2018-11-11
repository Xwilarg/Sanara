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
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Collections.Generic;
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
            if (!R.DbList().Contains(dbName).Run<bool>(conn))
                R.DbCreate(dbName).Run(conn);
            if (!R.Db(dbName).TableList().Contains("Users").Run<bool>(conn))
                R.Db(dbName).TableCreate("Users").Run(conn);
            if (!R.Db(dbName).TableList().Contains("Guilds").Run<bool>(conn))
                R.Db(dbName).TableCreate("Guilds").Run(conn);
        }

        public void InitGuild(ulong guildId)
        {
            string guildIdStr = guildId.ToString();
            if (R.Db(dbName).Table("Guilds").GetAll(guildIdStr).Count().Eq(0).Run<bool>(conn))
            {
                R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildIdStr)
                    .With("Prefix", "s.")
                    .With("Language", "en")
                    ).Run(conn);
            }
            dynamic json = R.Db(dbName).Table("Guilds").Get(guildIdStr).Run(conn);
            Languages.Add(guildId, (string)json.Language);
            Prefixs.Add(guildId, (string)json.Prefix);
        }

        public void SetPrefix(ulong guildId, string prefix)
        {
            R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("Prefix", prefix)
                ).Run(conn);
            Prefixs[guildId] = prefix;
        }

        public void SetLanguage(ulong guildId, string language)
        {
            R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("Language", language)
                ).Run(conn);
            Languages[guildId] = language;
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;

        public Dictionary<ulong, string> Languages { private set; get; }
        public Dictionary<ulong, string> Prefixs { private set; get; }
    }
}
