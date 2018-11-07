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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

namespace SanaraV2.Modules.Entertainment
{
    public class VnModule : ModuleBase
    {
        Program p = Program.p;

        [Command("Vn", RunMode = RunMode.Async)]
        public async Task Vndb(params string[] vns)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Vn);
            if (vns.Length == 0)
            {
                await ReplyAsync(Sentences.VndbHelp(Context.Guild.Id));
                return;
            }
            VisualNovel vn = await GetVn(Utilities.AddArgs(vns));
            if (vn == null || !Utilities.CleanWord(vn.Name).Contains(Utilities.CleanWord(Utilities.AddArgs(vns))))
            {
                await ReplyAsync(Sentences.VndbNotFound(Context.Guild.Id));
                return;
            }
            bool isNsfw = (Context.Channel as ITextChannel).IsNsfw;
            await ReplyAsync("", false, GetEmbed(vn, Context.Guild.Id, isNsfw));
            IGuildUser me = await Context.Guild.GetUserAsync(Base.Sentences.myId);
            if (me.GuildPermissions.AttachFiles)
            {
                foreach (string image in GetImages(vn, Context.Guild.Id, Context.User.Id, isNsfw))
                {
                    await Context.Channel.SendFileAsync(image);
                    File.Delete(image);
                }
            }
        }

        public static List<string> GetImages(VisualNovel vn, ulong guildId, ulong userId, bool isNsfw)
        {
            List<string> images = new List<string>();
            int counter = 0;
            foreach (ScreenshotMetadata image in vn.Screenshots.ToArray())
            {
                if ((!isNsfw && !image.IsNsfw) || isNsfw)
                {
                    using (WebClient wc = new WebClient())
                    {
                        string currName = "vn" + DateTime.Now.ToString("HHmmssfff") + guildId + userId + "." + image.Url.Split('.')[image.Url.Split('.').Length - 1];
                        wc.DownloadFile(image.Url, currName);
                        images.Add(currName);
                        counter++;
                        if (counter == 1)
                            break;
                    }
                }
            }
            return (images);
        }

        public static Embed GetEmbed(VisualNovel vn, ulong guildId, bool isNsfw)
        {
            List<string> tmpDesc = vn.Description.Split('\n').ToList();
            if (tmpDesc[tmpDesc.Count - 1].Contains("[/url]"))
                tmpDesc.RemoveAt(tmpDesc.Count - 1);
            string desc = String.Join(Environment.NewLine, tmpDesc);
            Dictionary<string, string> allLengths = new Dictionary<string, string>()
             {
                 { "VeryShort", "< 2 hours" },
                 { "Short", "2 - 10 hours" },
                 { "Medium", "10 - 30 hours" },
                 { "Long", "30 - 50 hours" },
                 { "VeryLong", "> 50 hours" }
             };
            string finalDesc = ((vn.Languages.ToArray().Contains("en")) ? (Sentences.AvailableEnglish(guildId)) : (Sentences.NotAvailableEnglish(guildId))) + Environment.NewLine
                 + ((vn.Platforms.Contains("win")) ? (Sentences.AvailableWindows(guildId)) : (Sentences.NotAvailableWindows(guildId))) + Environment.NewLine
                 + ((vn.Length != null) ? (vn.Length.ToString().Replace("Very", "Very ") + " (" + allLengths[vn.Length.ToString()] + ")" + Environment.NewLine) : (""))
                 + Sentences.VndbRating(guildId, vn.Rating.ToString()) + Environment.NewLine
                 + ((vn.Released.Year != null) ? ("Released" + ((vn.Released.Month != null) ? ((vn.Released.Day != null) ? (" the " + vn.Released.Day + "/" + vn.Released.Month + "/" + vn.Released.Year) : (" in " + vn.Released.Month + "/" + vn.Released.Year)) : (" in " + vn.Released.Year)) + ".") : ("Not released yet.")) + Environment.NewLine
                 + Environment.NewLine + Environment.NewLine
                 + desc;
            return (new EmbedBuilder()
            {
                Title = ((vn.OriginalName != null) ? (vn.OriginalName + " (" + vn.Name + ")") : (vn.Name)),
                ImageUrl = (((vn.IsImageNsfw && isNsfw) || !vn.IsImageNsfw) ? (vn.Image) : (null)),
                Description = finalDesc,
                Color = Color.Green
            }.Build());
        }

        public static async Task<VisualNovel> GetVn(string vnName)
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
            string cleanName = Utilities.CleanWord(vnName);
            string name = allVnsId.Find(x => Utilities.CleanWord(x).Contains(cleanName));
            try
            {
                if (name == null)
                    id = Convert.ToUInt32(Utilities.GetElementXml("a", "a" + allVnsId[1], '"'));
                else
                    id = Convert.ToUInt32(Utilities.GetElementXml("a", "a" + name, '"'));
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