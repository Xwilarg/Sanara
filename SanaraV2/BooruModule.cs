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

namespace SanaraV2
{
    public class BooruModule : ModuleBase
    {
        Program p = Program.p;

        private static string getTags(string[] tags)
        {
            string finalTags = "&tags=";
            if (tags.Length > 0)
            {
                finalTags += tags[0];
                for (int i = 1; i < tags.Length; i++)
                    finalTags += "+" + tags[i];
            }
            return (finalTags);
        }

        public abstract class Booru
        {
            public abstract int getNbMax(string tags);
            public abstract string getLink(string tags, int maxNb);
            public abstract string getFileUrl(string json);
            public abstract string[] getTagInfo(string tag);
            public virtual string getAllTags(string json) { return (Program.getElementXml("tags=\"", json, '"')); }
            public virtual string getTagName(string json) { return (Program.getElementXml("name=\"", json, '"')); }
            public virtual string getTagType(string json) { return (Program.getElementXml("type=\"", json, '"')); }
            public abstract BooruId getId();
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
            public override int getNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    return (Convert.ToInt32(Program.getElementXml("posts count=\"", wc.DownloadString("http://safebooru.org/index.php?page=dapi&s=post&q=index&limit=1" + tags), '"')) - 1);
                }
            }

            public override string getLink(string tags, int maxNb)
            {
                return ("https://safebooru.org/index.php?page=dapi&s=post&q=index&pid=" + (Program.p.rand.Next(maxNb) + 1) + tags + "&limit=1");
            }

            public override string getFileUrl(string json)
            {
                return ("https://" + Program.getElementXml("file_url=\"//", json, '"'));
            }

            public override string[] getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://safebooru.org/index.php?page=dapi&s=tag&q=index&name=" + tag).Split('<'));
                }
            }

            public override BooruId getId()
            {
                return (BooruId.Safebooru);
            }
        }

        public class Gelbooru : Booru
        {
            public override int getNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    int nbMax = Convert.ToInt32(Program.getElementXml("posts count=\"", wc.DownloadString("https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1" + tags), '"')) - 1;
                    if (nbMax > 20000)
                        return (20000 - 1);
                    else
                        return (nbMax);
                }
            }

            public override string getLink(string tags, int maxNb)
            {
                return ("https://gelbooru.com/index.php?page=dapi&s=post&q=index&pid=" + (Program.p.rand.Next(maxNb) + 1) + tags + "&limit=1");
            }

            public override string getFileUrl(string json)
            {
                return (Program.getElementXml("file_url=\"", json, '"'));
            }

            public override string[] getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://gelbooru.com/index.php?page=dapi&s=tag&q=index&name=" + tag).Split('<'));
                }
            }

            public override BooruId getId()
            {
                return (BooruId.Gelbooru);
            }
        }

        public class Konachan : Booru
        {
            public override int getNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    return (Convert.ToInt32(Program.getElementXml("posts count=\"", wc.DownloadString("https://www.konachan.com/post.xml?limit=1" + tags), '"')));
                }
            }

            public override string getLink(string tags, int maxNb)
            {
                return ("https://www.konachan.com/post.xml?page=" + (Program.p.rand.Next(maxNb) + 1) + tags + "&limit=1");
            }

            public override string getFileUrl(string json)
            {
                return (Program.getElementXml("file_url=\"", json, '"'));
            }

            public override string[] getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://konachan.com/tag.xml?limit=10000&name=" + tag).Split('<'));
                }
            }

            public override BooruId getId()
            {
                return (BooruId.Konachan);
            }
        }

        public class Rule34 : Booru
        {
            public override int getNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {

                    int nbMax = Convert.ToInt32(Program.getElementXml("posts count=\"", wc.DownloadString("https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=1" + tags), '"')) - 1;
                    if (nbMax > 20000)
                        return (20000 - 1);
                    else
                        return (nbMax);
                }
            }

            public override string getLink(string tags, int maxNb)
            {
                return ("https://rule34.xxx/index.php?page=dapi&s=post&q=index&pid=" + (Program.p.rand.Next(maxNb) + 1) + tags + "&limit=1");
            }

            public override string getFileUrl(string json)
            {
                return (Program.getElementXml("file_url=\"", json, '"'));
            }

            public override string[] getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://rule34.xxx/index.php?page=dapi&s=tag&q=index&name=" + tag).Split('<'));
                }
            }

            public override BooruId getId()
            {
                return (BooruId.Rule34);
            }
        }

        public class E621 : Booru
        {
            public override int getNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Sanara");
                    int nbMax = Convert.ToInt32(Program.getElementXml("<posts count=\"", wc.DownloadString("https://e621.net/post/index.xml?limit=1" + tags), '"')) - 1;
                    if (nbMax > 750)
                        return (750 - 1);
                    else
                        return (nbMax);
                }
            }

            public override string getLink(string tags, int maxNb)
            {
                return ("https://e621.net/post/index.xml?page=" + (Program.p.rand.Next(maxNb) + 1) + tags + "&limit=1");
            }

            public override string getFileUrl(string json)
            {
                return (Program.getElementXml("<file_url>", json, '<'));
            }

            public override string[] getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Sanara");
                    return (wc.DownloadString("https://e621.net/tag/index.xml?limit=10000&name=" + tag).Split(new string[] { "<tag>" }, StringSplitOptions.None));
                }
            }

            public override string getAllTags(string json)
            {
                return (Program.getElementXml("<tags>", json, '<'));
            }

            public override string getTagName(string json)
            {
                return (Program.getElementXml("<name>", json, '<'));
            }
            public override string getTagType(string json)
            {
                return (Program.getElementXml("<type type=\"integer\">", json, '<'));
            }

            public override BooruId getId()
            {
                return (BooruId.E621);
            }
        }

#pragma warning disable CS1998
        [Command("Safebooru", RunMode = RunMode.Async), Summary("Get an image from Safebooru")]
        public async Task safebooruSearch(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new Safebooru(), tags, Context.Channel as ITextChannel, currName, true, false);
        }

        [Command("Gelbooru", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task gelbooruSearch(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new Gelbooru(), tags, Context.Channel as ITextChannel, currName, false, false);
        }

        [Command("Konachan", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task konachanSearch(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new Konachan(), tags, Context.Channel as ITextChannel, currName, false, false);
        }

        [Command("Rule34", RunMode = RunMode.Async), Summary("Get an image from Rule34")]
        public async Task rule34Search(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new Rule34(), tags, Context.Channel as ITextChannel, currName, false, false);
        }

        [Command("E621", RunMode = RunMode.Async), Summary("Get an image from E621")]
        public async Task e621Search(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new E621(), tags, Context.Channel as ITextChannel, currName, false, false);
        }
#pragma warning restore CS1998

        /// <summary>
        /// Get an image given various informations
        /// </summary>
        /// <param name="booru">Which booru is concerned (see above)</param>
        /// <param name="tags">Tags that need to be contain on the image</param>
        /// <param name="chan">Channel the image will be post in</param>
        /// <param name="currName">Temporary name of the file</param>
        /// <param name="isSfw">Is the channel safe for work ?</param>
        /// <param name="isGame">If the request from Game module (doesn't count dl for stats and don't get informations about tags)</param>
        public static async void getImage(Booru booru, string[] tags, ITextChannel chan, string currName, bool isSfw, bool isGame)
        {
            if (!isSfw && !chan.IsNsfw)
            {
                await chan.SendMessageAsync(Sentences.chanIsNotNsfw(chan.GuildId));
                return;
            }
            IGuildUser me = await chan.Guild.GetUserAsync(Sentences.myId);
            if (!me.GuildPermissions.AttachFiles)
            {
                await chan.SendMessageAsync(Sentences.needAttachFile(chan.GuildId));
                return;
            }
            if (!isGame)
                await chan.SendMessageAsync(Sentences.prepareImage(chan.GuildId));
            string url = getBooruUrl(booru, tags);
            if (url == null)
                await chan.SendMessageAsync(Sentences.tagsNotFound(tags));
            else
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent: Sanara");
                    string json = wc.DownloadString(url);
                    string image = booru.getFileUrl(json);
                    string imageName = currName + "." + image.Split('.')[image.Split('.').Length - 1];
                    wc.Headers.Add("User-Agent: Sanara");
                    wc.DownloadFile(image, imageName);
                    FileInfo file = new FileInfo(imageName);
                    Program.p.statsMonth[(int)booru.getId()] += file.Length;
                    if (file.Length >= 8000000)
                        await chan.SendMessageAsync(Sentences.fileTooBig(chan.GuildId));
                    else
                    {
                        while (true)
                        {
                            try
                            {
                                await chan.SendFileAsync(imageName);
                                break;
                            }
                            catch (RateLimitedException) { }
                        }
                        if (!isGame)
                        {
                            List<string> finalStr = getTagsInfos(json, booru);
                            foreach (string s in finalStr)
                                await chan.SendMessageAsync(s);
                        }
                    }
                    File.Delete(imageName);
                }
                if (!isGame)
                {
                    string finalStrModule = "";
                    foreach (long i in Program.p.statsMonth)
                        finalStrModule += i + "|";
                    finalStrModule = finalStrModule.Substring(0, finalStrModule.Length - 1);
                    File.WriteAllText("Saves/MonthModules.dat", finalStrModule + Environment.NewLine + Program.p.lastMonthSent);
                }
            }
        }

        public static string getBooruUrl(Booru booru, string[] tags)
        {
            int maxVal = booru.getNbMax(getTags(tags));
            if (maxVal <= 0) // TODO: weird parsing sometimes (example hibiki_(kantai_collection) cut in half if not found)
                return (null);
            else
                return (booru.getLink(getTags(tags), maxVal));
        }

        private static List<string> getTagsInfos(string json, Booru booru)
        {
            List<string> animeFrom = new List<string>();
            List<string> characs = new List<string>();
            List<string> artists = new List<string>();
            string[] allTags = booru.getAllTags(json).Split(' ');
            using (WebClient w = new WebClient())
            {
                foreach (string t in allTags)
                {
                    foreach (string s in booru.getTagInfo(t))
                    {
                        if (booru.getTagName(s) == t)
                        {
                            switch (booru.getTagType(s))
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
            return (writeTagsInfos(animeFrom, characs, artists));
        }

        private static string fixName(string original)
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

        private static List<string> writeTagsInfos(List<string> animeFrom, List<string> characs, List<string> artists)
        {
            List<string> finalMsg = new List<string>();
            for (int i = 0; i < artists.Count; i++)
                artists[i] = fixName(artists[i]);
            for (int i = 0; i < characs.Count; i++)
                characs[i] = fixName(characs[i]);
            for (int i = 0; i < animeFrom.Count; i++)
                animeFrom[i] = fixName(animeFrom[i]);
            List<string> finalStrCharacs = new List<string>();
            finalStrCharacs.Add("");
            int indexCharacFrom = 0;
            if (characs.Count == 1)
                finalStrCharacs[indexCharacFrom] = characs[0];
            else if (characs.Count > 1)
            {
                bool doesContainTagMe = false;
                foreach (string s in characs)
                {
                    if (s == "Tagme" || s == "Character Request")
                        doesContainTagMe = true;
                }
                for (int i = 0; i < characs.Count - 1; i++)
                {
                    if (finalStrCharacs[indexCharacFrom].Length > 1500)
                    {
                        indexCharacFrom++;
                        finalStrCharacs.Add("");
                    }
                    if (characs[i] != "Tagme" && characs[i] != "Character Request")
                        finalStrCharacs[indexCharacFrom] += characs[i] + ", ";
                }
                if (!doesContainTagMe)
                {
                    finalStrCharacs[indexCharacFrom] = finalStrCharacs[indexCharacFrom].Substring(0, finalStrCharacs[indexCharacFrom].Length - 2);
                    finalStrCharacs[indexCharacFrom] += " and " + characs[characs.Count - 1];
                }
                else
                {
                    if (characs[characs.Count - 1] == "Tagme" || characs[characs.Count - 1] == "Source Request"
                        || characs[characs.Count - 1] == "Copyright Request")
                    {
                        finalStrCharacs[indexCharacFrom] = finalStrCharacs[indexCharacFrom].Substring(0, finalStrCharacs[indexCharacFrom].Length - 2);
                        finalStrCharacs[indexCharacFrom] += " and some other character who weren't tag";
                    }
                    else
                        finalStrCharacs[indexCharacFrom] += ", " + characs[characs.Count - 1] + " and some other character who weren't tag";
                }
            }
            List<string> finalStrFrom = new List<string>();
            int indexStrFrom = 0;
            finalStrFrom.Add("");
            if (animeFrom.Count == 1)
                finalStrFrom[indexStrFrom] = animeFrom[0];
            else if (animeFrom.Count > 1)
            {
                for (int i = 0; i < animeFrom.Count - 1; i++)
                    finalStrFrom[indexStrFrom] += animeFrom[i] + ", ";
                finalStrFrom[indexStrFrom] = finalStrFrom[indexStrFrom].Substring(0, finalStrFrom[indexStrFrom].Length - 2);
                finalStrFrom[indexStrFrom] += " and " + animeFrom[animeFrom.Count - 1];
                if (finalStrFrom[indexStrFrom].Length > 1500)
                {
                    indexStrFrom++;
                    finalStrFrom.Add("");
                }
            }
            string finalStr;
            if (animeFrom.Count == 1 && animeFrom[0] == "Original")
                finalStr = "It look like this image is an original content." + Environment.NewLine;
            else if (animeFrom.Count == 1 && (animeFrom[0] == "Tagme" || animeFrom[0] == "Source Request" || animeFrom[0] == "Copyright Request"))
                finalStr = "It look like the source of this image wasn't tag." + Environment.NewLine;
            else if (finalStrFrom[0] != "")
            {
                finalStr = "I think this image is from ";
                foreach (string s in finalStrFrom)
                {
                    if (finalStr.Length + s.Length > 1500)
                    {
                        finalMsg.Add(finalStr);
                        finalStr = "";
                    }
                    finalStr += s;
                }
                finalStr += "." + Environment.NewLine;
            }
            else
                finalStr = "I don't know where this image is from." + Environment.NewLine;
            if (finalStrCharacs[0] == "")
                finalStr += "I don't know who are the characters." + Environment.NewLine;
            else if (characs.Count == 1 && (characs[0] == "Tagme" || characs[0] == "Character Request"))
                finalStr += "It look like the characters of this image weren't tag." + Environment.NewLine;
            else if (characs.Count == 1)
                finalStr += "I think the character is " + finalStrCharacs[0] + "." + Environment.NewLine;
            else
            {
                if (finalStr.Length > 1500)
                {
                    finalMsg.Add(finalStr);
                    finalStr = "";
                }
                finalStr += "I think the characters are ";
                foreach (string s in finalStrCharacs)
                {
                    if ((finalStr.Length + s.Length) > 1500)
                    {
                        finalMsg.Add(finalStr);
                        finalStr = "";
                    }
                    finalStr += s;
                }
                finalStr += "." + Environment.NewLine;
            }
            finalMsg.Add(finalStr);
            return (finalMsg);
        }
    }
}