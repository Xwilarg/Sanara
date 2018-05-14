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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

namespace SanaraV2
{
    public class VndbModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Vn", RunMode = RunMode.Async)]
        public async Task vndb(params string[] vns)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Vn);
            if (vns.Length == 0)
            {
                await ReplyAsync(Sentences.vndbHelp);
                return;
            }
            VisualNovel vn = await getVn(Program.addArgs(vns));
            if (vn == null || !Program.cleanWord(vn.Name).Contains(Program.cleanWord(Program.addArgs(vns))))
            {
                await ReplyAsync(Sentences.vndbNotFound);
                return;
            }
            List<string> tmpDesc = vn.Description.Split('\n').ToList();
            Console.WriteLine(tmpDesc.Count);
            if (tmpDesc[tmpDesc.Count - 1].Contains("[/url]"))
                tmpDesc.RemoveAt(tmpDesc.Count - 1);
            string desc = "";
            foreach (string s in tmpDesc)
            {
                desc += s + Environment.NewLine;
            }
            Dictionary<string, string> allLengths = new Dictionary<string, string>()
             {
                 { "VeryShort", "< 2 hours" },
                 { "Short", "2 - 10 hours" },
                 { "Medium", "10 - 30 hours" },
                 { "Long", "30 - 50 hours" },
                 { "VeryLong", "> 50 hours" }
             };
            string finalDesc = ((vn.OriginalName != null) ? ("**" + vn.OriginalName + "** (" + vn.Name + ")") : ("**" + vn.Name + "**")) + Environment.NewLine
                 + ((vn.Languages.ToArray().Contains("en")) ? ("Available") : ("Not available")) + " in english." + Environment.NewLine
                 + ((vn.Platforms.Contains("win")) ? ("Available") : ("Not available")) + " on Windows." + Environment.NewLine
                 + ((vn.Length != null) ? (vn.Length.ToString().Replace("Very", "Very ") + " (" + allLengths[vn.Length.ToString()] + ")" + Environment.NewLine) : (""))
                 + "It's rate " + vn.Rating + "/10 on VNDB." + Environment.NewLine
                 + ((vn.Released.Year != null) ? ("Released" + ((vn.Released.Month != null) ? ((vn.Released.Day != null) ? (" the " + vn.Released.Day + "/" + vn.Released.Month + "/" + vn.Released.Year) : (" in " + vn.Released.Month + "/" + vn.Released.Year)) : (" in " + vn.Released.Year))) : ("Not released yet.")) + Environment.NewLine
                 + Environment.NewLine + Environment.NewLine
                 + desc;
            bool isNsfw = (Context.Channel as ITextChannel).IsNsfw;
            EmbedBuilder embed = new EmbedBuilder()
            {
                ImageUrl = ((vn.IsImageNsfw && isNsfw || !vn.IsImageNsfw) ? (vn.Image) : (null)),
                Description = finalDesc,
                Color = Color.Green
            };
            await ReplyAsync("", false, embed.Build());
            IGuildUser me = await Context.Guild.GetUserAsync(Sentences.myId);
            if (me.GuildPermissions.AttachFiles)
            {
                int counter = 0;
                foreach (ScreenshotMetadata image in vn.Screenshots.ToArray())
                {
                    if ((!isNsfw && !image.IsNsfw) || isNsfw)
                    {
                        using (WebClient wc = new WebClient())
                        {
                            string currName = "vn" + DateTime.Now.ToString("HHmmssfff") + Context.Guild.ToString() + Context.User.Id.ToString() + "." + image.Url.Split('.')[image.Url.Split('.').Length - 1];
                            wc.DownloadFile(image.Url, currName);
                            await Context.Channel.SendFileAsync(currName);
                            File.Delete(currName);
                            counter++;
                            if (counter == 1)
                                break;
                        }
                    }
                }
            }
        }
        public static async Task<VisualNovel> getVn(string vnName)
        {
            Vndb client = new Vndb();

            uint id = 0;
            HttpWebRequest http = (HttpWebRequest)WebRequest.Create("https://vndb.org/v/all?sq=" + vnName.Replace(' ', '+'));
            http.AllowAutoRedirect = false;
            string html;
            using (HttpWebResponse response = (HttpWebResponse)http.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            html = html.Split(new string[] { "<div class=\"mainbox browse vnbrowse\">" }, StringSplitOptions.None)[1];
            html = html.Split(new string[] { "</thead>" }, StringSplitOptions.None)[1];
            List<string> allVnsId = html.Split(new string[] { "href=\"/v" }, StringSplitOptions.None).ToList();
            string cleanName = Program.cleanWord(vnName);
            string name = allVnsId.Find(x => Program.cleanWord(x).Contains(cleanName));
            try
            {
                if (name == null)
                    id = Convert.ToUInt32(Program.getElementXml("a", "a" + allVnsId[1], '"'));
                else
                    id = Convert.ToUInt32(Program.getElementXml("a", "a" + name, '"'));
            }
            catch (FormatException)
            {
                return (null);
            }
            VndbResponse<VisualNovel> visualNovels = await client.GetVisualNovelAsync(VndbFilters.Id.Equals(id), VndbFlags.FullVisualNovel);
            return (visualNovels.ToArray()[0]);
        }
    }
}