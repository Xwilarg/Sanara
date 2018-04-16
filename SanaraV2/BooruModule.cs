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

namespace SanaraV2
{
    public class BooruModule : ModuleBase
    {
        Program p = Program.p;

        public string getTags(string[] tags)
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

        private abstract class Booru
        {
            public abstract int getNbMax(string tags);
            public abstract string getLink(string tags, int maxNb);
            public abstract string getFileUrl(string json);
            public abstract string getTagInfo(string tag);
        }

        private class Safebooru : Booru
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

            public override string getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://safebooru.org/index.php?page=dapi&s=tag&q=index&name=" + tag));
                }
            }
        }

        private class Gelbooru : Booru
        {
            public override int getNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {
                    int nbMax = Convert.ToInt32(Program.getElementXml("posts count=\"", wc.DownloadString("https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=1" + tags), '"')) - 1;
                    if (nbMax > 20000)
                        return (20000);
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

            public override string getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://gelbooru.com/index.php?page=dapi&s=tag&q=index&name=" + tag));
                }
            }
        }

        private class Konachan : Booru
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

            public override string getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://konachan.com/tag.xml?limit=10000&name=" + tag));
                }
            }
        }

        private class Rule34 : Booru
        {
            public override int getNbMax(string tags)
            {
                using (WebClient wc = new WebClient())
                {

                    int nbMax = Convert.ToInt32(Program.getElementXml("posts count=\"", wc.DownloadString("https://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=1" + tags), '"')) - 1;
                    if (nbMax > 20000)
                        return (20000);
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

            public override string getTagInfo(string tag)
            {
                using (WebClient wc = new WebClient())
                {
                    return (wc.DownloadString("https://rule34.xxx/index.php?page=dapi&s=tag&q=index&name=" + tag));
                }
            }
        }

        [Command("Safebooru", RunMode = RunMode.Async), Summary("Get an image from Safebooru")]
        public async Task safebooruSearch(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new Safebooru(), tags, Context.Channel, currName, true);
        }

        [Command("Gelbooru", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task gelbooruSearch(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new Gelbooru(), tags, Context.Channel, currName, false);
        }

        [Command("Konachan", RunMode = RunMode.Async), Summary("Get an image from Gelbooru")]
        public async Task konachanSearch(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new Konachan(), tags, Context.Channel, currName, false);
        }

        [Command("Rule34", RunMode = RunMode.Async), Summary("Get an image from Rule34")]
        public async Task rule34Search(params string[] tags)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Booru);
            string currName = "booru" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString();
            getImage(new Rule34(), tags, Context.Channel, currName, false);
        }

        private async void getImage(Booru booru, string[] tags, IMessageChannel chan, string currName, bool isSfw)
        {
            if (!isSfw && !chan.IsNsfw)
            {
                await chan.SendMessageAsync(Sentences.chanIsNotNsfw);
                return;
            }
            IGuildUser me = await Context.Guild.GetUserAsync(Sentences.myId);
            if (!me.GuildPermissions.AttachFiles)
            {
                await chan.SendMessageAsync(Sentences.needAttachFile);
                return;
            }
            await ReplyAsync(Sentences.prepareImage);
            int maxVal = booru.getNbMax(getTags(tags));
            if (maxVal <= 0) // TODO: weird parsing sometimes (example hibiki_(kantai_collection) cut in half if not found)
                await chan.SendMessageAsync(Sentences.tagsNotFound(tags));
            else
            {
                using (WebClient wc = new WebClient())
                {
                    string json = wc.DownloadString(booru.getLink(getTags(tags), maxVal));
                    string image = booru.getFileUrl(json);
                    string imageName = currName + "." + image.Split('.')[image.Split('.').Length - 1];
                    wc.DownloadFile(image, imageName);
                    FileInfo file = new FileInfo(imageName);
                    if (file.Length >= 8000000)
                        await ReplyAsync(Sentences.fileTooBig);
                    else
                    {
                        await chan.SendFileAsync(imageName);
                        List<string> finalStr = getTagsInfos(json, booru);
                        foreach (string s in finalStr)
                            await ReplyAsync(s);
                    }
                    File.Delete(imageName);
                }
            }
        }

        private List<string> getTagsInfos(string json, Booru booru)
        {
            List<string> animeFrom = new List<string>();
            List<string> characs = new List<string>();
            List<string> artists = new List<string>();
            string[] allTags = Program.getElementXml("tags=\"", json, '"').Split(' ');
            using (WebClient w = new WebClient())
            {
                foreach (string t in allTags)
                {
                    foreach (string s in booru.getTagInfo(t).Split('<'))
                    {
                        if (Program.getElementXml("name=\"", s, '"') == t)
                        {
                            switch (Program.getElementXml("type=\"", s, '"'))
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

        private string fixName(string original)
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

        private List<string> writeTagsInfos(List<string> animeFrom, List<string> characs, List<string> artists)
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