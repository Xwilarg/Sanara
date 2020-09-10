using Discord;
using Discord.WebSocket;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using SanaraV3.Subscription;
using SanaraV3.Subscription.Tags;
using System;
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
            _subscriptions = new Dictionary<string, Dictionary<ulong, SubscriptionGuild>>();
            _subscriptions.Add("anime", new Dictionary<ulong, SubscriptionGuild>());
            _subscriptions.Add("nhentai", new Dictionary<ulong, SubscriptionGuild>());
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
                    _subscriptions["anime"].Add(sGuild.Id, new SubscriptionGuild(sub.Item1, new AnimeTags(new string[0], false)));
                sub = await GetSubscriptionAsync(sGuild, "nhentai");
                if (sub != null)
                    _subscriptions["nhentai"].Add(sGuild.Id, new SubscriptionGuild(sub.Item1, new NHentaiTags(sub.Item2, false)));
            }
            _guilds.Add(sGuild.Id, guild);
        }

        // SUBSCRIPTIONS

        public async Task SetSubscriptionAsync(ulong guildId, string name, ITextChannel chan, ASubscriptionTags tags)
        {
            if (_subscriptions[name].ContainsKey(guildId))
                _subscriptions[name][guildId] = new SubscriptionGuild(chan, tags);
            else
                _subscriptions[name].Add(guildId, new SubscriptionGuild(chan, tags));
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(name + "Subscription", chan.Id)
            ).RunAsync(_conn);
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(name + "SubscriptionTags", chan.Id)
            ).RunAsync(_conn);
        }

        private async Task<Tuple<ITextChannel, string[]>> GetSubscriptionAsync(SocketGuild sGuild, string name)
        {
            if (await _r.Db(_dbName).Table("Guilds").GetAll(sGuild.Id.ToString()).GetField(name + "Subscription").Count().Eq(0).RunAsync<bool>(_conn))
                return null;
            var tmp = (Cursor<string>)await _r.Db(_dbName).Table("Guilds").GetAll(sGuild.Id.ToString()).GetField(name + "Subscription").RunAsync<string>(_conn);
            tmp.MoveNext();
            string sub = tmp.Current == "0" ? null : tmp.Current;
            if (sub == null) // No subscription
                return null;
            var chan = sGuild.GetTextChannel(ulong.Parse(sub));
            if (chan == null) // Text channel not available
                return null;
            string[] tags = await _r.Db(_dbName).Table("Guilds").GetAll(sGuild.Id.ToString()).GetField(name + "SubscriptionTags").RunAsync<string[]>(_conn);
            return new Tuple<ITextChannel, string[]>(chan, tags);
        }

        // PREFIX

        public async Task UpdatePrefixAsync(ulong guildId, string prefix)
        {
            _guilds[guildId].Prefix = prefix;
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With("Prefix", prefix)
            ).RunAsync(_conn);
        }

        // SCORES

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
        private Dictionary<string, Dictionary<ulong, SubscriptionGuild>> _subscriptions;
        public Guild GetGuild(ulong id) => _guilds[id];
    }
}
