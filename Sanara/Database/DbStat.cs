namespace Sanara.Database
{
    public sealed partial class Db
    {
        public async Task InitStatsAsync()
        {
            await CreateIfDontExistsAsync(_statDbName, "GuildCount");
            await CreateIfDontExistsAsync(_statDbName, "Errors");
            await CreateIfDontExistsAsync(_statDbName, "Commands");
            await CreateIfDontExistsAsync(_statDbName, "Games");
            await CreateIfDontExistsAsync(_statDbName, "GamesPlayers");
            await CreateIfDontExistsAsync(_statDbName, "Booru");
        }

        private string GetStatKey(string format)
        {
            return DateTime.UtcNow.ToString(format);
        }

        private async Task InsertOrAddAsync(string table, string key, string field)
        {
            if (await _r.Db(_statDbName).Table(table).GetAll(key).Count().Eq(0).RunAsync<bool>(_conn))
                await _r.Db(_statDbName).Table(table).Insert(_r.HashMap("id", key)
                        .With(field, 1)
                    ).RunAsync(_conn);
            else if (!await _r.Db(_statDbName).Table(table).Get(key).HasFields(field).RunAsync<bool>(_conn))
                await _r.Db(_statDbName).Table(table).Update(_r.HashMap("id", key)
                        .With(field, 1)
                    ).RunAsync(_conn);
            else
                await _r.Db(_statDbName).Table(table).Update(_r.HashMap("id", key)
                        .With(field, await _r.Db(_statDbName).Table(table).Get(key).GetField(field).Add(1).RunAsync(_conn))
                    ).RunAsync(_conn);
        }

        public async Task AddNewCommandAsync(string name)
        {
            await InsertOrAddAsync("Errors", Daily, "OK");
            await InsertOrAddAsync("Commands", Hourly, name);
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
