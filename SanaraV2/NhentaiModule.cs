
using Discord;
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class NhentaiModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Doujinshi", RunMode = RunMode.Async), Summary("Give a random doujinshi using nhentai API")]
        public async Task getNhentai(params string[] keywords)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Nhentai);
            if (!(Context.Channel as ITextChannel).IsNsfw)
            {
                await ReplyAsync(Sentences.chanIsNotNsfw);
                return;
            }
            string tags = "";
            if (keywords.Length != 0)
            {
                foreach (string s in keywords)
                {
                    tags += s + "+";
                }
                tags = tags.Substring(0, tags.Length - 1);
            }
            string xml;
            using (WebClient w = new WebClient())
            {
                w.Encoding = Encoding.UTF8;
                if (keywords.Length == 0)
                    xml = w.DownloadString("https://nhentai.net/api/galleries/all?page=0");
                else
                    xml = w.DownloadString("https://nhentai.net/api/galleries/search?query=" + tags + "&page=8000");
            }
            int page = p.rand.Next(Convert.ToInt32(Program.getElementXml("\"num_pages\":", xml, ','))) + 1;
            using (WebClient w = new WebClient())
            {
                w.Encoding = Encoding.UTF8;
                if (keywords.Length == 0)
                    xml = w.DownloadString("https://nhentai.net/api/galleries/all?page=" + page);
                else
                    xml = w.DownloadString("https://nhentai.net/api/galleries/search?query=" + tags + "&page=" + page);
            }
            List<string> allDoujinshi = xml.Split(new string[] { "title" }, StringSplitOptions.None).ToList();
            allDoujinshi.RemoveAt(0);
            if (allDoujinshi.Count == 0)
            {
                string[] allTags = tags.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                await ReplyAsync(Sentences.tagsNotFound(allTags));
            }
            else
            {
                string curr = allDoujinshi[p.rand.Next(allDoujinshi.Count)];
                string[] ids = curr.Split(new string[] { "}]" }, StringSplitOptions.None);
                string currBlock = "";
                for (int i = ids.Length - 1; i >= 0; i--)
                {
                    currBlock = Program.getElementXml("id\":", ids[i], ',');
                    if (currBlock != "")
                    {
                        if (keywords.Length == 0)
                            await ReplyAsync("https://nhentai.net/g/" + currBlock);
                        else
                        {
                            string finalOk = "";
                            foreach (string t in keywords)
                            {
                                bool isOk = false;
                                foreach (string s in ids[i - 1].Split(new string[] { "},{" }, StringSplitOptions.None))
                                {
                                    if (Program.getElementXml("\"name\":\"", s, '"').Contains(t))
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
                                await ReplyAsync("https://nhentai.net/g/" + currBlock);
                            else
                                await ReplyAsync(Sentences.tagsNotFound(new string[] { finalOk }));
                        }
                        break;
                    }
                }
            }
        }
    }
}