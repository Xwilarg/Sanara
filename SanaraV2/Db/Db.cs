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
using Discord;
using Newtonsoft.Json;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using SanaraV2.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2.Db
{
    public partial class Db
    {
        public Db()
        {
            R = RethinkDB.R;
            Languages = new Dictionary<ulong, string>();
            Prefixs = new Dictionary<ulong, string>();
            Availability = new Dictionary<ulong, string>();
            AnimeSubscription = new List<(ITextChannel, Subscription.SubscriptionTags)>();
            NHentaiSubscription = new List<(ITextChannel, Subscription.SubscriptionTags)>();
            Anonymize = new Dictionary<ulong, bool>();
        }

        public async Task InitAsync()
            => await InitAsync("Sanara");

        public async Task InitAsync(string dbName)
        {
            this.dbName = dbName;
            conn = await R.Connection().ConnectAsync();
            if (!await R.DbList().Contains(dbName).RunAsync<bool>(conn))
                R.DbCreate(dbName).Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Guilds").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Guilds").Run(conn);
            if (!await R.Db(dbName).TableList().Contains("Profiles").RunAsync<bool>(conn))
                R.Db(dbName).TableCreate("Profiles").Run(conn);
            await PreloadProfiles();
        }

        private static readonly string defaultAvailability = "1111111111111111";

        public async Task ResetGuild(ulong guildId)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                   .With("Prefix", "s.")
                   .With("Language", "en")
                   .With("Availability", defaultAvailability)
                   ).RunAsync(conn);
        }

        public async Task InitGuild(IGuild guild)
        {
            if (Languages.ContainsKey(guild.Id)) // If somehow InitGuild is called 2 times for the same guild we ignore it
                return;
            string guildIdStr = guild.Id.ToString();
            if (await R.Db(dbName).Table("Guilds").GetAll(guildIdStr).Count().Eq(0).RunAsync<bool>(conn))
            {
                await R.Db(dbName).Table("Guilds").Insert(R.HashMap("id", guildIdStr)
                    .With("Prefix", "s.")
                    .With("Language", "en")
                    .With("Availability", defaultAvailability)
                    ).RunAsync(conn);
            }
            dynamic json = await R.Db(dbName).Table("Guilds").Get(guildIdStr).RunAsync(conn);
            Languages.Add(guild.Id, (string)json.Language);
            Prefixs.Add(guild.Id, (string)json.Prefix);
            string availability = (string)json.Availability;
            if (availability == null)
                Availability.Add(guild.Id, defaultAvailability);
            else
            {
                string newAvailability = availability;
                while (newAvailability.Length < defaultAvailability.Length)
                    newAvailability += "1";
                Availability.Add(guild.Id, newAvailability);
            }
            string anonymize = (string)json.Anonymize;
            if (anonymize != null)
                Anonymize.Add(guild.Id, bool.Parse(anonymize));
            else
                Anonymize.Add(guild.Id, false);
            string anime = (string)json.animeSubscription;
            if (anime != null && anime != "0")
                AnimeSubscription.Add((await guild.GetTextChannelAsync(ulong.Parse(anime)), null));
            string nhentai = (string)json.nhentaiSubscription;
            if (nhentai != null && nhentai != "0")
                NHentaiSubscription.Add((await guild.GetTextChannelAsync(ulong.Parse(nhentai)), Subscription.SubscriptionTags.ParseSubscriptionTags(json.nhentaiSubscriptionTags.ToObject<string[]>())));
        }

        public async Task AddAnimeSubscription(ITextChannel chan)
        {
            string guildIdStr = chan.GuildId.ToString();
            string channelIdStr = chan.Id.ToString();
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildIdStr)
                .With("animeSubscription", channelIdStr)
                ).RunAsync(conn);
            AnimeSubscription.Add((chan, null));
        }

        public async Task AddNHentaiSubscription(ITextChannel chan, Subscription.SubscriptionTags tags)
        {
            string guildIdStr = chan.GuildId.ToString();
            string channelIdStr = chan.Id.ToString();
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildIdStr)
                .With("nhentaiSubscription", channelIdStr)
                .With("nhentaiSubscriptionTags", tags.ToStringArray())
                ).RunAsync(conn);
            NHentaiSubscription.Add((chan, tags));
        }

        public async Task<bool> RemoveAnimeSubscription(IGuild guild)
        {
            string guildIdStr = guild.Id.ToString();
            dynamic json = await R.Db(dbName).Table("Guilds").Get(guildIdStr).RunAsync(conn);
            string anime = (string)json.animeSubscription;
            if (anime == null || anime == "0")
                return false;
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildIdStr)
                .With("animeSubscription", "0")
                ).RunAsync(conn);
            AnimeSubscription.Remove((await guild.GetTextChannelAsync(ulong.Parse(anime)), null));
            return true;
        }

        public async Task<bool> RemoveNHentaiSubscription(IGuild guild)
        {
            string guildIdStr = guild.Id.ToString();
            dynamic json = await R.Db(dbName).Table("Guilds").Get(guildIdStr).RunAsync(conn);
            string hentai = (string)json.nhentaiSubscription;
            if (hentai == null || hentai == "0")
                return false;
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildIdStr)
                .With("nhentaiSubscription", "0")
                ).RunAsync(conn);
            NHentaiSubscription.Remove((await guild.GetTextChannelAsync(ulong.Parse(hentai)), Subscription.SubscriptionTags.ParseSubscriptionTags(json.nhentaiSubscriptionTags.ToObject<string[]>())));
            return true;
        }

        public async Task<string> GetMyChannelNameAnimeAsync(IGuild guild)
        {
            dynamic json = await R.Db(dbName).Table("Guilds").Get(guild.Id.ToString()).RunAsync(conn);
            string anime = (string)json.animeSubscription;
            if (anime != null)
            {
                ITextChannel chan = await guild.GetTextChannelAsync(ulong.Parse(anime));
                if (chan != null)
                {
                    return chan.Mention;
                }
            }
            return "None";
        }

        public async Task<string> GetMyChannelNameDoujinshiAsync(IGuild guild)
        {
            dynamic json = await R.Db(dbName).Table("Guilds").Get(guild.Id.ToString()).RunAsync(conn);
            string nhentai = (string)json.nhentaiSubscription;
            if (nhentai != null)
            {
                ITextChannel chan = await guild.GetTextChannelAsync(ulong.Parse(nhentai));
                if (chan != null)
                {
                    return chan.Mention;
                }
            }
            return "None";
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

        public async Task SetAvailability(ulong guildId, Program.Module module, int enable)
        {
            StringBuilder availability = new StringBuilder(Availability[guildId]);
            availability[(int)module] = (char)(enable + '0');
            string res = availability.ToString();
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("Availability", res)
                ).RunAsync(conn);
            Availability[guildId] = res;
        }

        public async Task SetAnonymize(ulong guildId, bool value)
        {
            await R.Db(dbName).Table("Guilds").Update(R.HashMap("id", guildId.ToString())
                .With("Anonymize", value)
                ).RunAsync(conn);
            Anonymize[guildId] = value;
        }

        public bool IsAvailable(ulong guildId, Program.Module module)
            => Availability[guildId][(int)module] == '1';

        public bool AreAllAvailable(ulong guildId)
            => Availability[guildId].All(x => x == '1');

        public bool AreNoneAvailable(ulong guildId)
            => Availability[guildId].Count(x => x == '0') == 2;

        public bool IsAnonymized(ulong guildId)
        {
            if (!Anonymize.ContainsKey(guildId))
                return false;
            return Anonymize[guildId];
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

        /// <returns>
        /// Dictionary<string, Dictionary<string, string>>
        /// Key: Guild id
        /// Value: Dictionnary of Key: game name, Value: score
        /// </returns>
        public async Task<Dictionary<string, Dictionary<string, string>>> GetAllScores()
        {
            Dictionary<string, Dictionary<string, string>> allScores = new Dictionary<string, Dictionary<string, string>>();
            var json = await R.Db(dbName).Table("Guilds").RunAsync(conn);
            foreach (var elem in json)
            {
                Dictionary<string, string> currDict = new Dictionary<string, string>();
                foreach (var game in Constants.allRankedGames)
                {
                    APreload preload = (APreload)Activator.CreateInstance(game.Item1);
                    string gameName = preload.GetGameName();
                    if (elem[gameName] != null) currDict.Add(gameName, elem[gameName].ToString());
                }
                if (currDict.Count > 0)
                    allScores.Add((string)elem.id, currDict);
            }
            return allScores;
        }

        private RethinkDB R; public RethinkDB GetR() => R;
        private Connection conn;
        private string dbName;

        public Dictionary<ulong, string> Languages { private set; get; }
        public Dictionary<ulong, string> Prefixs { private set; get; }
        public Dictionary<ulong, bool> Anonymize { private set; get; }
        public Dictionary<ulong, string> Availability { private set; get; }
        public List<(ITextChannel, Subscription.SubscriptionTags)> AnimeSubscription { private set; get; } // For each guild, their subscription channel
        public List<(ITextChannel, Subscription.SubscriptionTags)> NHentaiSubscription { private set; get; }
    }
}
