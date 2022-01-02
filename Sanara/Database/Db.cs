using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using Sanara.Game.Preload.Result;
using Sanara.Subscription;
using Sanara.Subscription.Tags;

namespace Sanara.Database
{
    public sealed partial class Db
    {
        public Db()
        {
            _r = RethinkDB.R;
            _guilds = new Dictionary<ulong, Guild>();
            _subscriptions = new Dictionary<string, Dictionary<ulong, SubscriptionGuild>>();
            _subscriptionProgress = new Dictionary<string, int>();

            _dbName = StaticObjects.BotName;
            _statDbName = StaticObjects.BotName + "-stats";
        }

        public async Task InitAsync()
        {
            _conn = await _r.Connection().ConnectAsync();

            // Creating dbs
            if (!await _r.DbList().Contains(_dbName).RunAsync<bool>(_conn))
                _r.DbCreate(_dbName).Run(_conn);
            if (!await _r.DbList().Contains(_statDbName).RunAsync<bool>(_conn))
                _r.DbCreate(_statDbName).Run(_conn);

            // Information about the different guilds
            await CreateIfDontExistsAsync(_dbName, "Guilds");

            // Current subscriptions
            await CreateIfDontExistsAsync(_dbName, "Subscriptions");

            await InitStatsAsync();

            var tmp = (Cursor<Subscription>)await _r.Db(_dbName).Table("Subscriptions").RunAsync<Subscription>(_conn);
            while (tmp.MoveNext())
                _subscriptionProgress.Add(tmp.Current.id, tmp.Current.value);
        }

        private async Task CreateIfDontExistsAsync(string dbName, string tableName)
        {
            if (!await _r.Db(dbName).TableList().Contains(tableName).RunAsync<bool>(_conn))
                _r.Db(dbName).TableCreate(tableName).Run(_conn);
        }

        public async Task InitGuildAsync(SocketGuild sGuild)
        {
            if (_guilds.ContainsKey(sGuild.Id)) // If the guild was already added, no need to do it a second time
                return;

            Guild guild;
            if (await _r.Db(_dbName).Table("Guilds").GetAll(sGuild.Id.ToString()).Count().Eq(0).RunAsync<bool>(_conn)) // Guild doesn't exist in db
            {
                guild = new Guild(sGuild.Id.ToString());
                await _r.Db(_dbName).Table("Guilds").Insert(guild).RunAsync(_conn);
            }
            else
            {
                guild = await _r.Db(_dbName).Table("Guilds").Get(sGuild.Id.ToString()).RunAsync<Guild>(_conn);
                var sub = await GetSubscriptionAsync(sGuild, "anime");
                if (sub != null)
                    _subscriptions["anime"].Add(sGuild.Id, new SubscriptionGuild(sub.Item1, new AnimeTags(Array.Empty<string>(), false)));
                sub = await GetSubscriptionAsync(sGuild, "nhentai");
                if (sub != null)
                    _subscriptions["nhentai"].Add(sGuild.Id, new SubscriptionGuild(sub.Item1, new NHentaiTags(sub.Item2, false)));
            }
            _guilds.Add(sGuild.Id, guild);

            // Get score
            var json = (JObject)await _r.Db(_dbName).Table("Guilds").Get(sGuild.Id.ToString()).RunAsync(_conn);
            foreach (var name in StaticObjects.AllGameNames)
            {
                if (json[name] != null)
                {
                    guild.UpdateScore(name, int.Parse(json[name].Value<string>().Split("|")[0]));
                }
            }
        }

        // AVAILABILITY

        public async Task AddAvailabilityAsync(ulong guildId, string name)
        {
            var g = _guilds[guildId];
            var tmp = g.AvailabilityModules.ToList();
            tmp.Remove(name);
            g.AvailabilityModules = tmp.ToArray();
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With("AvailabilityModules", tmp.ToArray())
            ).RunAsync(_conn);
        }

        public async Task RemoveAvailabilityAsync(ulong guildId, string name)
        {
            var g = _guilds[guildId];
            var tmp = g.AvailabilityModules.ToList();
            tmp.Add(name);
            g.AvailabilityModules = tmp.ToArray();
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With("AvailabilityModules", tmp.ToArray())
            ).RunAsync(_conn);
        }

        public bool IsAvailable(ulong guildId, string name)
        {
            var g = _guilds[guildId];
            return !g.AvailabilityModules.Contains(name);
        }

        // CACHES

        public async Task<QuizzPreloadResult[]> GetCacheAsync(string name)
        {
            if (!await _r.Db(_dbName).TableList().Contains("Cache_" + name).RunAsync<bool>(_conn))
                return Array.Empty<QuizzPreloadResult>();
            return ((Cursor<QuizzPreloadResult>)await _r.Db(_dbName).Table("Cache_" + name).RunAsync<QuizzPreloadResult>(_conn)).ToArray();
        }

        public async Task SetCacheAsync(string name, QuizzPreloadResult value)
        {
            if (!await _r.Db(_dbName).TableList().Contains("Cache_" + name).RunAsync<bool>(_conn))
                await _r.Db(_dbName).TableCreate("Cache_" + name).RunAsync(_conn);
            await _r.Db(_dbName).Table("Cache_" + name).Insert(value).RunAsync(_conn);
        }

        public async Task<bool> DeleteCacheAsync(string name)
        {
            name = name.ToLower();
            if (!await _r.Db(_dbName).TableList().Contains("Cache_" + name).RunAsync<bool>(_conn))
                return false;
            await _r.Db(_dbName).Table("Cache_" + name).Delete().RunAsync(_conn);
            return true;
        }

        // SUBSCRIPTIONS

        public int GetCurrent(string name)
        {
            if (!_subscriptionProgress.ContainsKey(name))
                return 0;
            return _subscriptionProgress[name];
        }

        public async Task SetCurrentAsync(string name, int value)
        {
            if (!_subscriptionProgress.ContainsKey(name))
                _subscriptionProgress.Add(name, value);
            else
                _subscriptionProgress[name] = value;
            if (await _r.Db(_dbName).Table("Subscriptions").GetAll(name).Count().Eq(0).RunAsync<bool>(_conn))
                await _r.Db(_dbName).Table("Subscriptions").Insert(new Subscription(name, value)).RunAsync(_conn);
            else
                await _r.Db(_dbName).Table("Subscriptions").Update(new Subscription(name, value)).RunAsync(_conn);
        }

        public void InitSubscription(string name)
        {
            _subscriptions.Add(name, new Dictionary<ulong, SubscriptionGuild>());
        }

        public async Task SetSubscriptionAsync(ulong guildId, string name, ITextChannel chan, ASubscriptionTags tags)
        {
            if (_subscriptions[name].ContainsKey(guildId))
                _subscriptions[name][guildId] = new SubscriptionGuild(chan, tags);
            else
                _subscriptions[name].Add(guildId, new SubscriptionGuild(chan, tags));
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(name + "Subscription", chan.Id.ToString())
            ).RunAsync(_conn);
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(name + "SubscriptionTags", tags.ToStringArray())
            ).RunAsync(_conn);
        }

        public async Task RemoveSubscriptionAsync(ulong guildId, string name)
        {
            if (_subscriptions[name].ContainsKey(guildId))
                _subscriptions[name].Remove(guildId);
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(name + "Subscription", "0")
            ).RunAsync(_conn);
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(name + "SubscriptionTags", Array.Empty<string>())
            ).RunAsync(_conn);
        }

        public async Task<bool> HasSubscriptionExistAsync(ulong guildId, string name)
        {
            if (_subscriptions[name].ContainsKey(guildId))
                return true;
            return !await _r.Db(_dbName).Table("Guilds").GetAll(guildId.ToString()).GetField(name + "Subscription").Count().Eq(0).RunAsync<bool>(_conn);
        }

        private async Task<Tuple<ITextChannel, string[]>?> GetSubscriptionAsync(SocketGuild sGuild, string name)
        {
            if (await _r.Db(_dbName).Table("Guilds").GetAll(sGuild.Id.ToString()).GetField(name + "Subscription").Count().Eq(0).RunAsync<bool>(_conn))
                return null;
            var tmp = (Cursor<string>)await _r.Db(_dbName).Table("Guilds").GetAll(sGuild.Id.ToString()).GetField(name + "Subscription").RunAsync<string>(_conn);
            tmp.MoveNext();
            string? sub = tmp.Current == "0" ? null : tmp.Current;
            if (sub == null) // No subscription
                return null;
            var chan = sGuild.GetTextChannel(ulong.Parse(sub));
            if (chan == null) // Text channel not available
                return null;
            if (await _r.Db(_dbName).Table("Guilds").GetAll(sGuild.Id.ToString()).GetField(name + "SubscriptionTags").Count().Eq(0).RunAsync<bool>(_conn))
                return new Tuple<ITextChannel, string[]>(chan, new string[0]);
            var tags = (Cursor<string[]>)await _r.Db(_dbName).Table("Guilds").GetAll(sGuild.Id.ToString()).GetField(name + "SubscriptionTags").RunAsync<string[]>(_conn);
            tags.MoveNext();
            return new Tuple<ITextChannel, string[]>(chan, tags.Current);
        }

        public SubscriptionGuild[] GetAllSubscriptions(string name)
            => _subscriptions[name].Values.ToArray();

        // PREFIX

        public async Task UpdatePrefixAsync(ulong guildId, string prefix)
        {
            _guilds[guildId].Prefix = prefix;
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With("Prefix", prefix)
            ).RunAsync(_conn);
        }

        public async Task UpdateFlagAsync(ulong guildId, bool value)
        {
            _guilds[guildId].TranslateUsingFlags = value;
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With("TranslateUsingFlags", value)
            ).RunAsync(_conn);
        }

        public async Task UpdateAnonymizeAsync(ulong guildId, bool value)
        {
            _guilds[guildId].Anonymize = value;
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With("Anonymize", value)
            ).RunAsync(_conn);
        }

        public async Task<string> DumpAsync(ulong guildId)
        {
            return ((object)await _r.Db(_dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(_conn)).ToString();
        }

        // SCORES

        public int GetGameScore(ulong guildId, string name, string argument)
        {
            string fullName = argument == null ? name : (name + "-" + argument);
            Guild g = _guilds[guildId];
            if (g.DoesContainsGame(fullName)) // Score already in cache
                return g.GetScore(fullName);
            return 0;
        }

        public async Task SaveGameScoreAsync(ulong guildId, int score, List<ulong> contributors, string name, string argument)
        {
            string fullName = argument == null ? name : (name + "-" + argument);
            _guilds[guildId].UpdateScore(fullName, score);
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(fullName, score + "|" + string.Join("|", contributors))
            ).RunAsync(_conn);
        }

        public List<int> GetAllScores(string gameName)
            => _guilds.Where(x => x.Value.DoesContainsGame(gameName)).Select(x => x.Value.GetScore(gameName)).ToList();

        private readonly RethinkDB _r;
        private Connection _conn;
        private string _dbName;

        private Dictionary<ulong, Guild> _guilds;
        private Dictionary<string, Dictionary<ulong, SubscriptionGuild>> _subscriptions; // All guild subscriptions
        private Dictionary<string, int> _subscriptionProgress;
        public Guild GetGuild(ulong id) => _guilds[id];
    }
}
