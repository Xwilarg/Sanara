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
using Discord.Commands;
using System;
using System.Net;
using Discord;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord.Net;
using System.Linq;
using SanaraV2.Base;

namespace SanaraV2.NSFW
{
    public class BooruModule : ModuleBase
    {
        Program p = Program.p;

        private static string GetTags(string[] tags)
        {
            string finalTags = "&tags=";
            if (tags.Length > 0)
            {
                finalTags += tags[0];
                if (tags.Length > 1)
                    finalTags += "+" + String.Join("+", tags.Skip(1));
            }
            return (finalTags);
        }

        public abstract class Booru
        {
            protected Booru(bool isSfw)
            {
                this.isSfw = isSfw;
            }
            public abstract int GetNbMax(string tags);
            public abstract string GetLink(string tags, int maxNb);
            public abstract string GetFileUrl(string json);
            public abstract string[] GetTagInfo(string tag);
            public virtual string GetAllTags(string json) { return (Utilities.GetElementXml("tags=\"", json, '"')); }
            public virtual string GetTagName(string json) { return (Utilities.GetElementXml("name=\"", json, '"')); }
            public virtual string GetTagType(string json) { return (Utilities.GetElementXml("type=\"", json, '"')); }
            public abstract BooruId GetId();
            public bool isSfw { private set; get; }
        }

        public enum BooruId
        {
            Safebooru,
            Gelbooru,
            Konachan,
            Rule34,
            E621
        }

        public class Safebooru : Booru
        {
            public Safebooru() : base(true)
            { }

            public override int GetNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    return (Convert.ToInt32(Utilities.GetElementXml("posts count=\"", wc.DownloadString("http://safebooru.org/index.php?page=dapi&s=post&q=index&limit=1" + tags), '"')));
                }
            }

            public override string GetLink(string tags, int maxNb)
            {
                return ("https://safebooru.org/index.php?page=dapi&s=post&q=index&pid=" + Program.p.rand.Next(maxNb) + tags + "&limit=1");
            }

            public override string GetFileUrl(string json)
            {
                return ("https://" + Utilities.GetElementXml("file_url=\"//", json, '"'));
            }

            public override string[] GetTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://safebooru.org/index.php?page=dapi&s=tag&q=index&name=" + tag).Split('<'));
                }
            }

            public override BooruId GetId()
            {
                return (BooruId.Safebooru);
            }
        }

        public class Gelbooru : Booru
        {
            public Gelbooru() : base(false)
            { }

            public override int GetNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    int nbMax = Convert.ToInt32(Utilities.GetElementXml("posts count=\"", wc.DownloadString("https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1" + tags), '"'));
                    if (nbMax > 20000)
                        return (20000 - 1);
                    else
                        return (nbMax);
                }
            }

            public override string GetLink(string tags, int maxNb)
            {
                return ("https://gelbooru.com/index.php?page=dapi&s=post&q=index&pid=" + Program.p.rand.Next(maxNb) + tags + "&limit=1");
            }

            public override string GetFileUrl(string json)
            {
                return (Utilities.GetElementXml("file_url=\"", json, '"'));
            }

            public override string[] GetTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://gelbooru.com/index.php?page=dapi&s=tag&q=index&name=" + tag).Split('<'));
                }
            }

            public override BooruId GetId()
            {
                return (BooruId.Gelbooru);
            }
        }

        public class Konachan : Booru
        {
            public Konachan() : base(false)
            { }

            public override int GetNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    return (Convert.ToInt32(Utilities.GetElementXml("posts count=\"", wc.DownloadString("https://www.konachan.com/post.xml?limit=1" + tags), '"')));
                }
            }

            public override string GetLink(string tags, int maxNb)
            {
                return ("https://www.konachan.com/post.xml?page=" + (Program.p.rand.Next(maxNb) + 1) + tags + "&limit=1");
            }

            public override string GetFileUrl(string json)
            {
                return (Utilities.GetElementXml("file_url=\"", json, '"'));
            }

            public override string[] GetTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://konachan.com/tag.xml?limit=10000&name=" + tag).Split('<'));
                }
            }

            public override BooruId GetId()
            {
                return (BooruId.Konachan);
            }
        }

        public class Rule34 : Booru
        {
            public Rule34() : base(false)
            { }

            public override int GetNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    int nbMax = Convert.ToInt32(Utilities.GetElementXml("posts count=\"", wc.DownloadString("https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=1" + tags), '"'));
                    if (nbMax > 20000)
                        return (20000 - 1);
                    else
                        return (nbMax);
                }
            }

            public override string GetLink(string tags, int maxNb)
            {
                return ("https://rule34.xxx/index.php?page=dapi&s=post&q=index&pid=" + Program.p.rand.Next(maxNb) + tags + "&limit=1");
            }

            public override string GetFileUrl(string json)
            {
                return (Utilities.GetElementXml("file_url=\"", json, '"'));
            }

            public override string[] GetTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://rule34.xxx/index.php?page=dapi&s=tag&q=index&name=" + tag).Split('<'));
                }
            }

            public override BooruId GetId()
            {
                return (BooruId.Rule34);
            }
        }

        public class E621 : Booru
        {
            public E621() : base(false)
            { }

            public override int GetNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Sanara");
                    int nbMax = Convert.ToInt32(Utilities.GetElementXml("<posts count=\"", wc.DownloadString("https://e621.net/post/index.xml?limit=1" + tags), '"'));
                    if (nbMax > 750)
                        return (750 - 1);
                    else
                        return (nbMax);
                }
            }

            public override string GetLink(string tags, int maxNb)
            {
                return ("https://e621.net/post/index.xml?page=" + (Program.p.rand.Next(maxNb) + 1) + tags + "&limit=1");
            }

            public override string GetFileUrl(string json)
            {
                return (Utilities.GetElementXml("<file_url>", json, '<'));
            }

            public override string[] GetTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Sanara");
                    return (wc.DownloadString("https://e621.net/tag/index.xml?limit=10000&name=" + tag).Split(new string[] { "<tag>" }, StringSplitOptions.None));
                }
            }

            public override string GetAllTags(string json)
            {
                return (Utilities.GetElementXml("<tags>", json, '<'));
            }

            public override string GetTagName(string json)
            {
                return (Utilities.GetElementXml("<name>", json, '<'));
            }
            public override string GetTagType(string json)
            {
                return (Utilities.GetElementXml("<type type=\"integer\">", json, '<'));
            }

            public override BooruId GetId()
            {
                return (BooruId.E621);
            }
        }

        [Command("Safebooru", RunMode = RunMode.Async), Summary("Get an image from Safebooru")]
        public async Task SafebooruSearch(params string[] tags)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString();
            await PostImage(new Safebooru(), Context.Channel as ITextChannel, tags);
        }

        [Command("Gelbooru", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task GelbooruSearch(params string[] tags)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString();
            await PostImage(new Gelbooru(), Context.Channel as ITextChannel, tags);
        }

        [Command("Konachan", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task KonachanSearch(params string[] tags)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString();
            await PostImage(new Konachan(), Context.Channel as ITextChannel, tags);
        }

        [Command("Rule34", RunMode = RunMode.Async), Summary("Get an image from Rule34")]
        public async Task Rule34Search(params string[] tags)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString();
            await PostImage(new Rule34(), Context.Channel as ITextChannel, tags);
        }

        [Command("E621", RunMode = RunMode.Async), Summary("Get an image from E621")]
        public async Task E621Search(params string[] tags)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.Id.ToString() + Context.User.Id.ToString();
            await PostImage(new E621(), Context.Channel as ITextChannel, tags);
        }

        private static async Task PostImage(Booru booru, ITextChannel chan, string[] tags)
        {
            if (!booru.isSfw && !chan.IsNsfw)
            {
                await chan.SendMessageAsync(Base.Sentences.ChanIsNotNsfw(chan.GuildId));
                return;
            }
            IGuildUser me = await chan.Guild.GetUserAsync(Base.Sentences.myId);
            if (!me.GuildPermissions.AttachFiles)
            {
                if (chan != null)
                    await chan.SendMessageAsync(Base.Sentences.NeedAttachFile(chan.GuildId));
                return;
            }
            await chan.SendMessageAsync(Sentences.PrepareImage(chan.GuildId));
            string fileName = booru.GetId().ToString() + DateTime.Now.ToString("HHmmssfff") + chan.Id + Program.p.rand.Next(int.MaxValue);
            long fileSize;
            string json;
            string url = GetImageUrl(booru, tags, out json);
            if (json == null)
            {
                await chan.SendMessageAsync(url);
                return;
            }
            DownloadFile(ref fileName, out fileSize, url);
            Program.p.statsMonth[(int)booru.GetId()] += fileSize;
            Program.p.statsMonth[(int)booru.GetId() + 5]++;
            if (fileSize >= 8000000)
                await chan.SendMessageAsync(Sentences.FileTooBig(chan.GuildId));
            else
            {
                IUserMessage msg;
                while (true)
                {
                    try
                    {
                        await chan.SendFileAsync(fileName);
                        msg = await chan.SendMessageAsync(".");
                        break;
                    }
                    catch (RateLimitedException)
                    { }
                }
                File.Delete(fileName);
                await msg.ModifyAsync(x => x.Content = GetTagsInfos(json, booru, (chan == null) ? (0) : (chan.GuildId)));
                File.WriteAllText("Saves/MonthModules.dat", String.Join("|", Program.p.statsMonth) + Environment.NewLine + Program.p.lastMonthSent);
            }
        }

        public static string GetImage(Booru booru, string[] tags)
        {
            string fileName = booru.GetId().ToString() + DateTime.Now.ToString("HHmmssfff") + Program.p.rand.Next(int.MaxValue);
            long fileSize;
            DownloadFile(ref fileName, out fileSize, GetImageUrl(booru, tags, out _));
            if (fileSize >= 8000000)
                return (null);
            else
                return (fileName);
        }

        public static string GetImageUrl(Booru booru, string[] tags, out string json)
        {
            string url = GetBooruUrl(booru, tags);
            if (url == null)
            {
                json = null;
                return (Base.Sentences.TagsNotFound(tags));
            }
            else
                return (GetFileUrl(booru, url, out json));
        }

        public static void DownloadFile(ref string fileName, out long fileSize, string url)
        {
            using (WebClient wc = new WebClient())
            {
                fileName += "." + url.Split('.')[url.Split('.').Length - 1];
                wc.Headers.Add("User-Agent: Sanara");
                wc.DownloadFile(url, fileName);
                FileInfo file = new FileInfo(fileName);
                fileSize = file.Length;
            }
        }

        private static string GetFileUrl(Booru booru, string url, out string json)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("User-Agent: Sanara");
                json = wc.DownloadString(url);
                string image = booru.GetFileUrl(json);
                return (image);
            }
        }

        public static string GetBooruUrl(Booru booru, string[] tags)
        {
            int maxVal = booru.GetNbMax(GetTags(tags));
            if (maxVal <= 0)
                return (null);
            else
                return (booru.GetLink(GetTags(tags), maxVal));
        }

        public static string GetTagsInfos(string json, Booru booru, ulong guildId)
        {
            List<string> animeFrom = new List<string>();
            List<string> characs = new List<string>();
            List<string> artists = new List<string>();
            string[] allTags = booru.GetAllTags(json).Split(' ');
            using (WebClient w = new WebClient())
            {
                foreach (string t in allTags)
                {
                    foreach (string s in booru.GetTagInfo(t))
                    {
                        if (booru.GetTagName(s) == t)
                        {
                            switch (booru.GetTagType(s))
                            {
                                case "1":
                                    artists.Add(t);
                                    break;

                                case "3":
                                    animeFrom.Add(t);
                                    break;

                                case "4":
                                    characs.Add(t);
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
            return (WriteTagsInfos(animeFrom, characs, artists, guildId));
        }

        private static string FixName(string original)
        {
            original = Regex.Replace(original, @"\(([^\)]+)\)", "");
            string newName = "";
            bool isPreviousSpace = true;
            foreach (char c in original)
            {
                if (isPreviousSpace)
                    newName += char.ToUpper(c);
                else if (c == '_')
                    newName += ' ';
                else
                    newName += c;
                isPreviousSpace = (c == '_');
            }
            newName = newName.Trim();
            return newName;
        }

        private static string WriteTagsInfos(List<string> animeFrom, List<string> characs, List<string> artists, ulong guildId)
        {
            artists = artists.Select(x => FixName(x)).ToList();
            characs = characs.Select(x => FixName(x)).ToList();
            animeFrom = animeFrom.Select(x => FixName(x)).ToList();

            artists.RemoveAll(x => artists.Count(delegate (string s) { return (Utilities.CleanWord(s) == Utilities.CleanWord(x)); }) > 1);
            characs.RemoveAll(x => characs.Count(delegate (string s) { return (Utilities.CleanWord(s) == Utilities.CleanWord(x)); }) > 1);
            animeFrom.RemoveAll(x => animeFrom.Count(delegate (string s) { return (Utilities.CleanWord(s) == Utilities.CleanWord(x)); }) > 1);

            string finalStr = GetAnimes(animeFrom, guildId) + Environment.NewLine;
            finalStr += GetCharacs(characs, guildId) + Environment.NewLine;
            finalStr += GetArtists(artists, guildId);
            return (finalStr);
        }

        private static string GetCharacs(List<string> characs, ulong guildId)
        {
            string finalCharacs = "";
            if (characs.Count == 1)
                finalCharacs = characs[0];
            else if (characs.Count > 1)
            {
                bool doesContainTagMe = characs.Any(x => x == "Tagme" || x == "Character Request");
                for (int i = 0; i < characs.Count - 1; i++)
                {
                    if (finalCharacs.Length > 850)
                    {
                        finalCharacs += Sentences.AndSomeOthers(guildId);
                        break;
                    }
                    if (characs[i] != "Tagme" && characs[i] != "Character Request")
                        finalCharacs += characs[i] + ", ";
                }
                if (!doesContainTagMe)
                {
                    finalCharacs = finalCharacs.Substring(0, finalCharacs.Length - 2);
                    finalCharacs += " " + Base.Sentences.AndStr(guildId) + " " + characs[characs.Count - 1];
                }
                else
                {
                    if (characs[characs.Count - 1] == "Tagme" || characs[characs.Count - 1] == "Source Request"
                        || characs[characs.Count - 1] == "Copyright Request")
                    {
                        finalCharacs = finalCharacs.Substring(0, finalCharacs.Length - 2);
                        finalCharacs += Sentences.CharacterNotTagged(guildId);
                    }
                    else
                        finalCharacs += ", " + characs[characs.Count - 1] + Sentences.MoreNotTagged(guildId);
                }
            }
            if (finalCharacs == "")
                return (Sentences.CharacterTagUnknowed(guildId));
            if (characs.Count == 1 && (characs[0] == "Tagme" || characs[0] == "Character Request"))
                return (Sentences.CharacterNotTagged(guildId));
            if (characs.Count == 1)
                return (Sentences.CharacterIs(guildId) + finalCharacs + ".");
            return (Sentences.CharacterAre(guildId) + finalCharacs + ".");
        }

        private static string GetAnimes(List<string> animeFrom, ulong guildId)
        {
            string finalStr = "";
            if (animeFrom.Count == 1)
                finalStr = animeFrom[0];
            else if (animeFrom.Count > 1)
            {
                for (int i = 0; i < animeFrom.Count - 1; i++)
                {
                    if (finalStr.Length > 850)
                    {
                        finalStr += Sentences.AndSomeOthers(guildId);
                        break;
                    }
                    finalStr += animeFrom[i] + ", ";
                }
            }
            finalStr = finalStr.Substring(0, finalStr.Length - 2);
            finalStr += " " + Base.Sentences.AndStr(guildId) + " " + animeFrom[animeFrom.Count - 1];
            if (animeFrom.Count == 1 && animeFrom[0] == "Original")
                return (Sentences.AnimeFromOriginal(guildId));
            if (animeFrom.Count == 1 && (animeFrom[0] == "Tagme" || animeFrom[0] == "Source Request" || animeFrom[0] == "Copyright Request"))
                return (Sentences.AnimeNotTagged(guildId));
            if (finalStr != "")
                return (Sentences.AnimeFrom(guildId) + finalStr);
            return (Sentences.AnimeTagUnknowed(guildId));
        }

        private static string GetArtists(List<string> artists, ulong guildId)
        {
            if (artists.Count > 0)
            {
                string finalStr = Sentences.ArtistFrom(guildId);
                if (artists.Count > 1)
                {
                    finalStr += String.Join(", ", artists.Take(artists.Count - 1));
                    finalStr += " " + Base.Sentences.AndStr(guildId) + " " + artists[artists.Count - 1];
                }
                else
                    finalStr += artists[0];
                return (finalStr);
            }
            else
                return (Sentences.ArtistNotTagged(guildId));
        }
    }
}