﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using Sanara.Game;
using Sanara.Game.Preload.Result;
using Sanara.Subscription;
using Sanara.Subscription.Tags;
using System.Net.Sockets;
using System.Text.Json;

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
            string botName =
#if NSFW_BUILD
                "Sanara";
#else
                "Hanaki";
#endif
            _dbName = botName;
            _statDbName = botName + "_stats";
        }

        public async Task InitAsync()
        {
            try
            {
                _conn = await _r.Connection().ConnectAsync();
            }
            catch (SocketException)
            {
                throw new InvalidOperationException("Failed to connect to db, make sure rethinkdb is started");
            }

            // Creating dbs
            if (!await _r.DbList().Contains(_dbName).RunAsync<bool>(_conn))
                _r.DbCreate(_dbName).Run(_conn);
            if (!await _r.DbList().Contains(_statDbName).RunAsync<bool>(_conn))
                _r.DbCreate(_statDbName).Run(_conn);

            // Information about the different guilds
            await CreateIfDontExistsAsync(_dbName, "Guilds");

            // Current subscriptions
            await CreateIfDontExistsAsync(_dbName, "Subscriptions");

            // Global information
            await CreateIfDontExistsAsync(_dbName, "Data");

            await InitStatsAsync();

            var tmp = (Cursor<Subscription>)await _r.Db(_dbName).Table("Subscriptions").RunAsync<Subscription>(_conn);
            while (tmp.MoveNext())
                _subscriptionProgress.Add(tmp.Current.id, tmp.Current.value);

            if (await _r.Db(_dbName).Table("Data").GetAll("currentDay").Count().Eq(0).RunAsync<bool>(_conn))
            {
                var currDay = DateTime.UtcNow.ToString("yyyyMMdd");
                await _r.Db(_dbName).Table("Data").Insert(_r.HashMap("id", "currentDay")
                        .With("value", currDay)
                    ).RunAsync(_conn);
                CurrentDay = currDay;
            }
            else
            {
                CurrentDay = (await _r.Db(_dbName).Table("Data").Get("currentDay").RunAsync(_conn))["value"];
            }
        }

        public async Task<bool> CheckForDayUpdateAsync()
        {
            var newDay = DateTime.UtcNow.ToString("yyyyMMdd");
            if (newDay == CurrentDay) return false;
            CurrentDay = newDay;
            await _r.Db(_dbName).Table("Data").Update(_r.HashMap("id", "currentDay")
                       .With("value", newDay)
                   ).RunAsync(_conn);
            return true;
        }

        private async Task CreateIfDontExistsAsync(string dbName, string tableName)
        {
            if (!await _r.Db(dbName).TableList().Contains(tableName).RunAsync<bool>(_conn))
                _r.Db(dbName).TableCreate(tableName).Run(_conn);
        }

        public async Task InitGuildAsync(IServiceProvider provider, SocketGuild sGuild)
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
                    _subscriptions["anime"].Add(sGuild.Id, new SubscriptionGuild(sub.Item1, new DefaultTags(Array.Empty<string>(), false)));
                sub = await GetSubscriptionAsync(sGuild, "inspire");
                if (sub != null)
                    _subscriptions["inspire"].Add(sGuild.Id, new SubscriptionGuild(sub.Item1, new DefaultTags(Array.Empty<string>(), false)));
                //sub = await GetSubscriptionAsync(sGuild, "nhentai");
                //if (sub != null)
                //    _subscriptions["nhentai"].Add(sGuild.Id, new SubscriptionGuild(sub.Item1, new NHentaiTags(sub.Item2, false)));
            }
            _guilds.Add(sGuild.Id, guild);

            // Get score
            var json = (JObject)await _r.Db(_dbName).Table("Guilds").Get(sGuild.Id.ToString()).RunAsync(_conn);
            foreach (var name in provider.GetRequiredService<GameManager>().AllGameNames.Select(x => GetCacheName(x)))
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

        public string GetCacheName(string name)
        {
            name = Utils.CleanWord(name);
            if (name.EndsWith("quizz"))
            {
                name = name[..^5]; // Retro compatibility with V3
            }
            return name;
        }

        public async Task<QuizzPreloadResult[]> GetCacheAsync(string name)
        {
            name = GetCacheName(name);
            if (!await _r.Db(_dbName).TableList().Contains("Cache_" + name).RunAsync<bool>(_conn))
            {
                await Log.LogAsync(new(LogSeverity.Verbose, "Database", $"Cache of {name} requested but is empty"));
                return Array.Empty<QuizzPreloadResult>();
            }
            var cache = ((Cursor<QuizzPreloadResult>)await _r.Db(_dbName).Table("Cache_" + name).RunAsync<QuizzPreloadResult>(_conn)).ToArray();
            await Log.LogAsync(new(LogSeverity.Verbose, "Database", $"Cache of {name} requested, {cache.Length} elements"));
            return cache;
        }

        public async Task SetCacheAsync(string name, QuizzPreloadResult value)
        {
            name = GetCacheName(name);
            await Log.LogAsync(new(LogSeverity.Verbose, "Database", $"Cache of {name} updated, {value.Answers.Length} elements"));
            if (!await _r.Db(_dbName).TableList().Contains("Cache_" + name).RunAsync<bool>(_conn))
                await _r.Db(_dbName).TableCreate("Cache_" + name).RunAsync(_conn);
            await _r.Db(_dbName).Table("Cache_" + name).Insert(value).RunAsync(_conn);
        }

        public async Task<bool> DeleteCacheAsync(string name)
        {
            name = GetCacheName(name);
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
            return JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(((object)await _r.Db(_dbName).Table("Guilds").Get(guildId.ToString()).RunAsync(_conn)).ToString()), new JsonSerializerOptions()
            {
                WriteIndented = true
            });
        }

        // SCORES

        public int GetGameScore(ulong guildId, string name, string argument)
        {
            string fullName = argument == null ? GetCacheName(name) : (GetCacheName(name) + "-" + argument);
            Guild g = _guilds[guildId];
            if (g.DoesContainsGame(fullName)) // Score already in cache
                return g.GetScore(fullName);
            return 0;
        }

        public async Task SaveGameScoreAsync(ulong guildId, int score, List<string> contributors, string name, string argument)
        {
            string fullName = argument == null ? GetCacheName(name) : (GetCacheName(name) + "-" + argument);
            _guilds[guildId].UpdateScore(fullName, score);
            await _r.Db(_dbName).Table("Guilds").Update(_r.HashMap("id", guildId.ToString())
                .With(fullName, score + "|" + string.Join("|", contributors))
            ).RunAsync(_conn);
        }

        public List<int> GetAllScores(string gameName)
        {
            gameName = GetCacheName(gameName);
            return _guilds.Where(x => x.Value.DoesContainsGame(gameName)).Select(x => x.Value.GetScore(gameName)).ToList();
        }

        private readonly RethinkDB _r;
        private Connection _conn;
        private string _dbName;

        private Dictionary<ulong, Guild> _guilds;
        private Dictionary<string, Dictionary<ulong, SubscriptionGuild>> _subscriptions; // All guild subscriptions
        private Dictionary<string, int> _subscriptionProgress;
        public Guild GetGuild(ulong id) => _guilds[id];

        public string CurrentDay { private set; get; }
    }
}
