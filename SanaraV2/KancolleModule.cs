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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class KancolleModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Kancolle", RunMode = RunMode.Async), Summary("Get informations about a Kancolle character")]
        public async Task charac(params string[] shipNameArr)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            if (shipNameArr.Length == 0)
            {
                await ReplyAsync(Sentences.kancolleHelp);
                return;
            }
            string shipName = Program.addArgs(shipNameArr);
            IGuildUser me = await Context.Guild.GetUserAsync(329664361016721408); // Sanara
            string url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + shipName + "&limit=1";
            try
            {
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    List<string> finalStr = new List<string>();
                    finalStr.Add("");
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    string json = w.DownloadString(url);
                    string code = Program.getElementXml("\"id\":", json, ',');
                    url = "http://kancolle.wikia.com/api/v1/Articles/Details?ids=" + code;
                    json = w.DownloadString(url);
                    string image = Program.getElementXml("\"thumbnail\":\"", json, '"');
                    if (Program.getElementXml("\"title\":\"", json, '"').ToUpper() != shipName.ToUpper())
                    {
                        await ReplyAsync(Sentences.shipgirlDontExist);
                        return;
                    }
                    url = "http://kancolle.wikia.com/wiki/" + Program.getElementXml("\"title\":\"", json, '"') + "?action=raw";
                    json = w.DownloadString(url);
                    if (Program.getElementXml("{{", json, '}') != "ShipPageHeader" && Program.getElementXml("{{", json, '}') != "Ship/Header")
                    {
                        await ReplyAsync(Sentences.shipgirlDontExist);
                        return;
                    }
                    int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                    if (me.GuildPermissions.AttachFiles)
                    {
                        image = image.Split(new string[] { ".jpg" }, StringSplitOptions.None)[0] + ".jpg";
                        image = image.Replace("\\", "");
                        w.DownloadFile(image, "shipgirl" + currentTime + ".jpg");
                    }
                    url = "http://kancolle.wikia.com/api/v1/Articles/AsSimpleJson?id=" + code;
                    json = w.DownloadString(url);
                    string[] jsonInside = json.Split(new string[] { "\"title\"" }, StringSplitOptions.None);
                    int currI = 0;
                    foreach (string s in jsonInside)
                    {
                        if (s.Contains("Personality"))
                        {
                            finalStr[0] += "**Personality**" + Environment.NewLine;
                            string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                            foreach (string str in allExplanations)
                            {
                                string per = Program.getElementXml("xt\":\"", str, '"');
                                if (per != "")
                                    finalStr[0] += per + Environment.NewLine;
                            }
                            break;
                        }
                    }
                    foreach (string s in jsonInside)
                    {
                        if (s.Contains("Appearance"))
                        {
                            finalStr[0] += Environment.NewLine + "**Appearance**" + Environment.NewLine;
                            string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                            foreach (string str in allExplanations)
                            {
                                string per = Program.getElementXml("xt\":\"", str, '"');
                                if (per != "")
                                {
                                    if (finalStr[currI].Length > 1500)
                                    {
                                        currI++;
                                        finalStr.Add("");
                                    }
                                    finalStr[currI] += per + Environment.NewLine;
                                }
                            }
                            break;
                        }
                    }
                    foreach (string s in jsonInside)
                    {
                        if (s.Contains("Second Remodel"))
                        {
                            finalStr[0] += "**Second Remodel**" + Environment.NewLine;
                            string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                            foreach (string str in allExplanations)
                            {
                                string per = Program.getElementXml("xt\":\"", str, '"');
                                if (per != "")
                                {
                                    if (finalStr[currI].Length > 1500)
                                    {
                                        currI++;
                                        finalStr.Add("");
                                    }
                                    finalStr[currI] += per + Environment.NewLine;
                                }
                            }
                            break;
                        }
                    }
                    foreach (string s in jsonInside)
                    {
                        if (s.Contains("Trivia"))
                        {
                            finalStr[currI] += Environment.NewLine + "**Trivia**" + Environment.NewLine;
                            string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                            foreach (string str in allExplanations)
                            {
                                string per = Program.getElementXml("xt\":\"", str, '"');
                                if (per != "")
                                {
                                    if (finalStr[currI].Length > 1500)
                                    {
                                        currI++;
                                        finalStr.Add("");
                                    }
                                    finalStr[currI] += per + Environment.NewLine;
                                }
                            }
                            break;
                        }
                    }
                    if (me.GuildPermissions.AttachFiles)
                        await Context.Channel.SendFileAsync("shipgirl" + currentTime + ".jpg");
                    foreach (string s in finalStr)
                    {
                        await ReplyAsync(Regex.Replace(
                        s,
                        @"\\[Uu]([0-9A-Fa-f]{4})",
                        m => char.ToString(
                            (char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)))); // Replace \\u1313 by \u1313
                    }
                    if (me.GuildPermissions.AttachFiles)
                        File.Delete("shipgirl" + currentTime + ".jpg");
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse code = ex.Response as HttpWebResponse;
                if (code.StatusCode == HttpStatusCode.NotFound)
                    await ReplyAsync(Sentences.shipgirlDontExist);
            }
        }
    }
}