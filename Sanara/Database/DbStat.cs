namespace Sanara.Database
{
    public sealed partial class Db
    {
        public async Task InitStatsAsync()
        {
            await CreateIfDontExistsAsync(_dbName, "GuildCount");
            await CreateIfDontExistsAsync(_dbName, "NbMessages");
            await CreateIfDontExistsAsync(_dbName, "Errors");
            await CreateIfDontExistsAsync(_dbName, "Commands");
            await CreateIfDontExistsAsync(_dbName, "Games");
            await CreateIfDontExistsAsync(_dbName, "GamesPlayers");
            await CreateIfDontExistsAsync(_dbName, "Booru");
        }

        private string GetStatKey(string format)
        {
            return DateTime.Now.ToString(format);
        }

        private async Task InsertOrAddAsync(string table, string key, string field)
        {
            if (await _r.Db(_statDbName).Table(table).GetAll(key).Count().Eq(0).RunAsync<bool>(_conn))
                await _r.Db(_statDbName).Table(table).Insert(_r.HashMap("id", key)
                    .With(field, 1)
                ).RunAsync(_conn);
            else
                await _r.Db(_statDbName).Table(table).Get(key).Update(_r.Row(key).Add(1)).RunAsync(_conn);
        }

        public async Task AddNewCommandAsync(string name)
        {
            await InsertOrAddAsync("NbMessages", Daily, StaticObjects.BotName);
            await InsertOrAddAsync("Errors", Daily, "OK");
            await InsertOrAddAsync("Commands", Daily, name);
        }

        public async Task AddErrorAsync(System.Exception e)
        {
            await InsertOrAddAsync("Errors", Daily, e.GetType().ToString());
        }

        public async Task AddGameAsync(string name, string option)
        {
            await InsertOrAddAsync("Games", Daily, name + (option == null ? "" : "-" + option));
        }

        public async Task AddGamePlayerAsync(string name, string option, int playerCount)
        {
            await InsertOrAddAsync("GamesPlayers", Daily, name + (option == null ? "" : "-" + option) + ";" + playerCount.ToString());
        }

        public async Task AddBooruAsync(string name)
        {
            await InsertOrAddAsync("Booru", Daily, name);
        }

        public async Task UpdateGuildCountAsync()
        {
            if (await _r.Db(_statDbName).Table("GuildCount").GetAll(Daily).Count().Eq(0).RunAsync<bool>(_conn))
                await _r.Db(_statDbName).Table("GuildCount").Insert(_r.HashMap("id", Daily)
                    .With("count", StaticObjects.Client.Guilds.Count)
                ).RunAsync(_conn);
            else
                await _r.Db(_statDbName).Table("GuildCount").Update(_r.HashMap("id", Daily)
                    .With("count", StaticObjects.Client.Guilds.Count)
                ).RunAsync(_conn);
        }

        public string Daily => GetStatKey("yyyyMMdd");
        public string Hourly => GetStatKey("yyyyMMddHH");

        private string _statDbName;
    }
}
