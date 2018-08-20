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
using Discord.Commands;
using SanaraV2.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2.NSFW
{
    public class DoujinshiModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Doujinshi", RunMode = RunMode.Async), Summary("Give a random doujinshi using nhentai API")]
        public async Task GetNhentai(params string[] keywords)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Doujinshi);
            if (!(Context.Channel as ITextChannel).IsNsfw)
            {
                await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                return;
            }
            string errorTag;
            string finalStr = GetDoujinshi(keywords, out errorTag);
            if (finalStr == null)
            {
                if (errorTag == null)
                    await ReplyAsync(Base.Sentences.TagsNotFound(keywords));
                else
                    await ReplyAsync(Base.Sentences.TagsNotFound(new string[] { errorTag }));
            }
            else
                await ReplyAsync(finalStr);
        }

        public static string GetDoujinshi(string[] keywords, out string wrongTag)
        {
            wrongTag = null;
            string tags = "";
            if (keywords.Length != 0)
                tags = String.Join("+", keywords);
            string xml = GetXml(keywords, tags);
            List<string> allDoujinshi = xml.Split(new string[] { "title" }, StringSplitOptions.None).ToList();
            allDoujinshi.RemoveAt(0);
            if (allDoujinshi.Count == 0)
                return (null);
            else
            {
                string curr = allDoujinshi[Program.p.rand.Next(allDoujinshi.Count)];
                string[] ids = curr.Split(new string[] { "}]" }, StringSplitOptions.None);
                string currBlock = "";
                for (int i = ids.Length - 1; i >= 0; i--)
                {
                    currBlock = Utilities.GetElementXml("\"id\":", ids[i], ',');
                    if (currBlock != "")
                    {
                        if (currBlock[currBlock.Length - 1] == '"')
                            return GetDoujinshi(keywords, out wrongTag);
                        if (keywords.Length == 0)
                        {
                            return ("https://nhentai.net/g/" + currBlock);
                        }
                        else
                        {
                            string finalOk = "";
                            foreach (string t in keywords)
                            {
                                bool isOk = false;
                                foreach (string s in ids[i - 1].Split(new string[] { "},{" }, StringSplitOptions.None))
                                {
                                    if (Utilities.GetElementXml("\"name\":\"", s, '"').Contains(t))
                                    {
                                        isOk = true;
                                        break;
                                    }
                                }
                                if (!isOk)
                                {
                                    finalOk = t;
                                    break;
                                }
                            }
                            if (finalOk == "")
                                return ("https://nhentai.net/g/" + currBlock);
                            else
                            {
                                wrongTag = finalOk;
                                return (null);
                            }
                        }
                    }
                }
                return (null);
            }
        }

        private static string GetXml(string[] keywords, string tags)
        {
            string xml;
            using (WebClient w = new WebClient())
            {
                w.Encoding = Encoding.UTF8;
                if (keywords.Length == 0)
                    xml = w.DownloadString("https://nhentai.net/api/galleries/all?page=0");
                else
                    xml = w.DownloadString("https://nhentai.net/api/galleries/search?query=" + tags + "&page=8000");
            }
            int page = Program.p.rand.Next(Convert.ToInt32(Utilities.GetElementXml("\"num_pages\":", xml, ','))) + 1;
            using (WebClient w = new WebClient())
            {
                w.Encoding = Encoding.UTF8;
                if (keywords.Length == 0)
                    return (w.DownloadString("https://nhentai.net/api/galleries/all?page=" + page));
                else
                    return (w.DownloadString("https://nhentai.net/api/galleries/search?query=" + tags + "&page=" + page));
            }
        }
    }
}