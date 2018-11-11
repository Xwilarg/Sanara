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
using System.Threading.Tasks;

namespace SanaraV2.Db
{
    public class Db
    {
        public Db()
        {
            R = RethinkDB.R;
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

        public void InitGuild(string guildId)
        {
            if (R.Db(dbName).Table("Guilds").GetAll(guildId).Count().Eq(0).Run<bool>(conn))
            {
                R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildId)
                    .With("Prefix", "s.")
                    .With("Language", "en")
                    ).Run(conn);
            }
        }

        private RethinkDB R;
        private Connection conn;
        private string dbName;
    }
}
