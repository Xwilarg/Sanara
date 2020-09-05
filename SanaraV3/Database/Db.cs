using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV3.Database
{
    public sealed class Db
    {
        public Db()
        {
            _r = RethinkDB.R;
            _guilds = new Dictionary<ulong, Guild>();
        }

        public async Task InitAsync(string dbName)
        {
            _dbName = dbName;
            _conn = await _r.Connection().ConnectAsync();
            if (!await _r.DbList().Contains(_dbName).RunAsync<bool>(_conn))
                _r.DbCreate(_dbName).Run(_conn);
            if (!await _r.Db(_dbName).TableList().Contains("Guilds").RunAsync<bool>(_conn))
                _r.Db(_dbName).TableCreate("Guilds").Run(_conn);
        }



        public async Task InitGuildAsync(ulong guildId)
        {
            if (_guilds.ContainsKey(guildId)) // If the guild was already added, no need to do it a second time
                return;

            Guild guild;
            if (await _r.Db(_dbName).Table("Guilds").GetAll(guildId.ToString()).Count().Eq(0).RunAsync<bool>(_conn)) // Guild doesn't exist in db
            {
                guild = new Guild(guildId.ToString());
                await _r.Db(_dbName).Table("Guilds").Insert(guild).RunAsync(_conn);
            }
            else
                guild = await _r.Db(_dbName).Table("Guilds").Get(guildId.ToString()).RunAsync<Guild>(_conn);
            _guilds.Add(guildId, guild);
        }

        public async Task UpdatePrefixAsync(ulong guildId, string prefix)
        {
            _guilds[guildId].Prefix = prefix;
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With("Prefix", prefix)
            ).RunAsync(_conn);
        }

        public async Task<int> GetGameScoreAsync(ulong guildId, string name, string argument)
        {
            string fullName = argument == null ? name : (name + "-" + argument);
            Guild g = _guilds[guildId];
            if (g.DoesContainsGame(fullName)) // Score already in cache
                return g.GetScore(fullName);
            if (await _r.Db(_dbName).Table("Guilds").GetAll(guildId.ToString()).GetField(fullName).Count().Eq(0).RunAsync<bool>(_conn)) // No score for this game in the db
                return 0;
            var tmp = (Cursor<string>)await _r.Db(_dbName).Table("Guilds").GetAll(guildId.ToString()).GetField(fullName).RunAsync<string>(_conn);
            tmp.MoveNext();
            return int.Parse(tmp.Current.Split("|")[0]);
        }

        public async Task SaveGameScoreAsync(ulong guildId, int score, List<ulong> contributors, string name, string argument)
        {
            string fullName = argument == null ? name : (name + "-" + argument);
            _guilds[guildId].UpdateScore(fullName, score);
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(fullName, score + "|" + string.Join("|", contributors))
            ).RunAsync(_conn);
        }

        private readonly RethinkDB _r;
        private Connection _conn;
        private string _dbName;

        private Dictionary<ulong, Guild> _guilds;
        public Guild GetGuild(ulong id) => _guilds[id];
    }
}
