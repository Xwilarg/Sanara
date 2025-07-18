using Discord.WebSocket;
using RevoltSharp;
using Sanara.Game;

namespace Sanara.Database
{
    public sealed partial class Db
    {
        public async Task InitStatsAsync()
        {
            await CreateIfDontExistsAsync(_statDbName, "GuildCount");
            await CreateIfDontExistsAsync(_statDbName, "Errors");
            await CreateIfDontExistsAsync(_statDbName, "Commands");
            await CreateIfDontExistsAsync(_statDbName, "CommandsDaily");
            await CreateIfDontExistsAsync(_statDbName, "GamesPlayers");
            await CreateIfDontExistsAsync(_statDbName, "Booru");
            await CreateIfDontExistsAsync(_statDbName, "Download");
            await CreateIfDontExistsAsync(_statDbName, "Platform");
        }

        private string GetStatKey(string format)
        {
            return DateTime.UtcNow.ToString(format);
        }

        private async Task InsertOrAddAsync(string table, string key, string field, int value = 1)
        {
            if (await _r.Db(_statDbName).Table(table).GetAll(key).Count().Eq(0).RunAsync<bool>(_conn))
                await _r.Db(_statDbName).Table(table).Insert(_r.HashMap("id", key)
                        .With(field, value)
                    ).RunAsync(_conn);
            else if (!await _r.Db(_statDbName).Table(table).Get(key).HasFields(field).RunAsync<bool>(_conn))
                await _r.Db(_statDbName).Table(table).Update(_r.HashMap("id", key)
                        .With(field, value)
                    ).RunAsync(_conn);
            else
                await _r.Db(_statDbName).Table(table).Update(_r.HashMap("id", key)
                        .With(field, await _r.Db(_statDbName).Table(table).Get(key).GetField(field).Add(value).RunAsync(_conn))
                    ).RunAsync(_conn);
        }

        public async Task AddNewCommandAsync(string name, bool isSlashCommand, string platform)
        {
            var data = $"{name};{(isSlashCommand ? 0 : 1)};{platform}";
            await InsertOrAddAsync("Commands", Hourly, data);
            await InsertOrAddAsync("CommandsDaily", Daily, data);
        }

        public async Task AddCommandSucceed()
        {
            await InsertOrAddAsync("Errors", Daily, "OK");
        }

        public async Task AddErrorAsync(System.Exception e)
        {
            await InsertOrAddAsync("Errors", Daily, e.GetType().ToString());
        }

        public async Task AddDownloadDoujinshiAsync(int size)
        {
            await InsertOrAddAsync("Download", Daily, "Doujinshi", size);
        }

        public async Task AddDownloadCosplayAsync(int size)
        {
            await InsertOrAddAsync("Download", Daily, "Cosplay", size);
        }

        public async Task AddGamePlayerAsync(string name, string option, int playerCount, MultiplayerType type)
        {
            await InsertOrAddAsync("GamesPlayers", Daily, name + (option == null ? "" : "-" + option) + ";" + playerCount.ToString() + ";" + type.ToString());
        }

        public async Task AddBooruAsync(string name)
        {
            await InsertOrAddAsync("Booru", Daily, name);
        }

        public async Task UpdateGuildCountAsync(DiscordSocketClient discordClient, RevoltClient revoltClient)
        {
            if (await _r.Db(_statDbName).Table("GuildCount").GetAll(Daily).Count().Eq(0).RunAsync<bool>(_conn))
            {
                await _r.Db(_statDbName).Table("GuildCount").Insert(_r.HashMap("id", Daily)).RunAsync(_conn);
            }
            if (discordClient != null)
            {
                await _r.Db(_statDbName).Table("GuildCount").Update(_r.HashMap("id", Daily)
                    .With("discord", discordClient.Guilds.Count)
                ).RunAsync(_conn);
            }
            if (revoltClient != null)
            {
                await _r.Db(_statDbName).Table("GuildCount").Update(_r.HashMap("id", Daily)
                    .With("revolt", revoltClient.Servers.Count)
                ).RunAsync(_conn);
            }

            if (await _r.Db(_statDbName).Table("GuildCount").GetAll("Latest").Count().Eq(0).RunAsync<bool>(_conn))
            {
                await _r.Db(_statDbName).Table("GuildCount").Insert(_r.HashMap("id", "Latest")).RunAsync(_conn);
            }
            if (discordClient != null)
            {
                await _r.Db(_statDbName).Table("GuildCount").Update(_r.HashMap("id", "Latest")
                    .With("discord", discordClient.Guilds.Count)
                ).RunAsync(_conn);
            }
            if (revoltClient != null)
            {
                await _r.Db(_statDbName).Table("GuildCount").Update(_r.HashMap("id", "Latest")
                    .With("revolt", revoltClient.Servers.Count)
                ).RunAsync(_conn);
            }
        }

        public string Daily => GetStatKey("yyyyMMdd");
        public string Hourly => GetStatKey("yyyyMMddHH");

        private string _statDbName;
    }
}
