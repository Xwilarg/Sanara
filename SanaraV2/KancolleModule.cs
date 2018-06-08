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
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class KancolleModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Map", RunMode = RunMode.Async), Summary("Get informations about a map")]
        public async Task map(params string[] mapName)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            if (mapName.Length != 2 || mapName[0].Length != 1 || mapName[1].Length != 1
                || mapName[0][0] <= '0' || mapName[0][0] > '6' || mapName[1][0] <= '0' || mapName[1][0] > '6'
                || (mapName[0][0] != '1' && mapName[1][0] == '6'))
            {
                await ReplyAsync(Sentences.mapHelp(Context.Guild.Id));
                return;
            }
            using (WebClient wc = new WebClient())
            {
                string url = "http://kancolle.wikia.com/wiki/World_" + mapName[0][0] + "/" + mapName[0][0] + "-" + mapName[1][0];
                string html = wc.DownloadString(url);
                wc.Encoding = Encoding.UTF8;
                string htmlRaw = wc.DownloadString(url + "?action=raw");
                html = html.Split(new string[] { "typography-xl-optout" }, StringSplitOptions.None)[1];
                string[] allLinks = html.Split(new string[] { "href=" }, StringSplitOptions.None);
                int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                wc.DownloadFile(Program.getElementXml("\"", allLinks[1], '"'), "kancolleMap" + currentTime + "1.png");
                wc.DownloadFile(Program.getElementXml("\"", allLinks[2], '"'), "kancolleMap" + currentTime + "2.png");
                await ReplyAsync(Program.getElementXml("|en = ", htmlRaw, '\n'));
                await Context.Channel.SendFileAsync("kancolleMap" + currentTime + "1.png");
                await Context.Channel.SendFileAsync("kancolleMap" + currentTime + "2.png");
                File.Delete("kancolleMap" + currentTime + "1.png");
                File.Delete("kancolleMap" + currentTime + "2.png");
                string branchingRules;
                if (htmlRaw.Contains("{{MapBranchingTable"))
                    branchingRules = htmlRaw.Split(new string[] { "{{MapBranchingTable" }, StringSplitOptions.None)[1];
                else
                    branchingRules = htmlRaw.Split(new string[] { "{{Map/Branching" }, StringSplitOptions.None)[1];
                string[] allBranches = branchingRules.Split(new string[] { "}}" }, StringSplitOptions.None)[0].Split('\n');
                string finalStr = "";
                foreach (string currBranch in allBranches)
                {
                    if (currBranch.Length == 0 || currBranch.StartsWith("|title") || currBranch.StartsWith("|id"))
                        continue;
                    string line = currBranch.Substring(1, currBranch.Length - 1);
                    finalStr += line + Environment.NewLine;
                }
                await ReplyAsync(finalStr);
            }
        }

        [Command("Drop", RunMode = RunMode.Async), Summary("Get informations about a drop")]
        public async Task drop(params string[] shipNameArr)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            if (shipNameArr.Length == 0)
            {
                await ReplyAsync(Sentences.kancolleHelp(Context.Guild.Id));
                return;
            }
            string shipName = Program.cleanWord(Program.addArgs(shipNameArr));
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                string html = wc.DownloadString("http://kancolle.wikia.com/wiki/Internals/Translations");
                List<string> allShipsName = html.Split(new string[] { "<tr" }, StringSplitOptions.None).ToList();
                allShipsName.RemoveAt(0);
                string shipContain = allShipsName.Find(x => Program.cleanWord(Program.getElementXml("\">", x, '<')) == shipName);
                if (shipContain == null)
                {
                    await ReplyAsync(Sentences.shipgirlDontExist(Context.Guild.Id));
                    return;
                }
                shipName = Program.getElementXml("<td>", shipContain, '<');
                html = wc.DownloadString("https://wikiwiki.jp/kancolle/%E8%89%A6%E5%A8%98%E3%83%89%E3%83%AD%E3%83%83%E3%83%97%E9%80%86%E5%BC%95%E3%81%8D");
                html = html.Split(new string[] { "<thead>" }, StringSplitOptions.None)[1];
                html = html.Split(new string[] { "艦種別表" }, StringSplitOptions.None)[0];
                string[] shipgirls = html.Split(new string[] { "<tr>" }, StringSplitOptions.None);
                string shipgirl = shipgirls.ToList().Find(x => x.Contains(shipName));
                string finalStr = "";
                if (shipgirl == null)
                    await ReplyAsync(Sentences.shipNotReferencedMap(Context.Guild.Id));
                else
                {
                    string[] cathegories = shipgirl.Split(new string[] { "<td class=\"style_td\"" }, StringSplitOptions.RemoveEmptyEntries);
                    int[] toKeep = new int[] { 4, 5, 6, 7, 8, 9,    // World 1
                                           11, 12, 13, 14, 15,  // world 2
                                           17, 18, 19, 20, 21,  // World 3
                                           23, 24, 25, 26, 27,  // World 4
                                           29, 30, 31, 32, 33,  // World 5
                                           35, 36, 37, 38, 39}; // World 6
                    int level = 1;
                    int world = 1;
                    foreach (int i in toKeep)
                    {
                        string node = Program.getElementXml(">", cathegories[i], '<');
                        if (node.Length > 0)
                        {
                            switch (node[0])
                            {
                                case '●':
                                    finalStr += world + "-" + level + ": " + Sentences.onlyNormalNodes(Context.Guild.Id) + Environment.NewLine;
                                    break;

                                case '○':
                                    finalStr += world + "-" + level + ": " + Sentences.onlyBossNode(Context.Guild.Id) + Environment.NewLine;
                                    break;

                                case '◎':
                                    finalStr += world + "-" + level + ": " + Sentences.anyNode(Context.Guild.Id) + Environment.NewLine;
                                    break;

                                default:
                                    finalStr += world + "-" + level + ": " + Sentences.defaultNode(Context.Guild.Id) + Environment.NewLine;
                                    break;
                            }
                        }
                        level++;
                        if ((world == 1 && level > 6) || (world > 1 && level > 5))
                        {
                            world++;
                            level = 1;
                        }
                    }
                    if (finalStr.Length > 0)
                    {
                        string rarity = Program.getElementXml(">", cathegories[1], '<');
                        await ReplyAsync(Sentences.rarity(Context.Guild.Id) + " " + ((rarity.Length > 0) ? (rarity) : ("?")) + "/7" + Environment.NewLine + finalStr);
                    }
                    else
                        await ReplyAsync(Sentences.dontDropOnMaps(Context.Guild.Id));
                }
                wc.Headers.Add("User-Agent: Sanara");
                html = wc.DownloadString("http://unlockacgweb.galstars.net/KanColleWiki/viewCreateShipLogList");
                html = Regex.Replace(
                        html,
                        @"\\[Uu]([0-9A-Fa-f]{4})",
                        m => char.ToString(
                            (char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier))); // Replace \\u1313 by \u1313
                string[] htmlSplit = html.Split(new string[] { shipName }, StringSplitOptions.None);
                if (htmlSplit.Length == 1)
                {
                    await ReplyAsync(Sentences.shipNotReferencedConstruction(Context.Guild.Id));
                    return;
                }
                string[] allIds = htmlSplit[htmlSplit.Length - 2].Split(new string[] { "\",\"" }, StringSplitOptions.None);
                wc.Headers.Add("User-Agent: Sanara");
                html = wc.DownloadString("http://unlockacgweb.galstars.net/KanColleWiki/viewCreateShipLog?sid=" + (allIds[allIds.Length - 1].Split('"')[0]));
                html = html.Split(new string[] { "order_by_probability" }, StringSplitOptions.None)[1];
                html = html.Split(new string[] { "flagship_order" }, StringSplitOptions.None)[0];
                string[] allElements = html.Split(new string[] { "{\"item1\":" }, StringSplitOptions.None);
                finalStr = Sentences.shipConstruction(Context.Guild.Id) + Environment.NewLine;
                for (int i = 1; i < ((allElements.Length > 6) ? (6) : (allElements.Length)); i++)
                {
                    string[] ressources = allElements[i].Split(new string[] { ",\"" }, StringSplitOptions.None);
                    finalStr += Program.getElementXml(":", ressources[7], '}') + "% -- " + Sentences.fuel(Context.Guild.Id) + " " + ressources[0] + ", " + Sentences.ammos(Context.Guild.Id) + " " + Program.getElementXml(":", ressources[1], '?') + ", " + Sentences.iron(Context.Guild.Id) + " " + Program.getElementXml(":", ressources[2], '?') + ", " + Sentences.bauxite(Context.Guild.Id) + " " + Program.getElementXml(":", ressources[3], '?')
                         + ", " + Sentences.devMat(Context.Guild.Id) + " " + Program.getElementXml(":", ressources[4], '?') + Environment.NewLine;
                }
                await ReplyAsync(finalStr);
            }
        }

        [Command("Kancolle", RunMode = RunMode.Async), Summary("Get informations about a Kancolle character")]
        public async Task charac(params string[] shipNameArr)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            if (shipNameArr.Length == 0)
            {
                await ReplyAsync(Sentences.kancolleHelp(Context.Guild.Id));
                return;
            }
            string shipName = Program.addArgs(shipNameArr);
            IGuildUser me = await Context.Guild.GetUserAsync(Sentences.myId);
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
                        await ReplyAsync(Sentences.shipgirlDontExist(Context.Guild.Id));
                        return;
                    }
                    url = "http://kancolle.wikia.com/wiki/" + Program.getElementXml("\"title\":\"", json, '"') + "?action=raw";
                    json = w.DownloadString(url);
                    if (Program.getElementXml("{{", json, '}') != "ShipPageHeader" && Program.getElementXml("{{", json, '}') != "Ship/Header")
                    {
                        await ReplyAsync(Sentences.shipgirlDontExist(Context.Guild.Id));
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
                            finalStr[0] += "**" + Sentences.personality(Context.Guild.Id) + "**" + Environment.NewLine;
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
                            finalStr[0] += Environment.NewLine + "**" + Sentences.appearance(Context.Guild.Id) + "**" + Environment.NewLine;
                            string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                            foreach (string str in allExplanations)
                            {
                                string per = Program.getElementXml("xt\":\"", str, '"');
                                if (per != "")
                                {
                                    if (finalStr[currI].Length + per.Length > 1500)
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
                            finalStr[0] += "**" + Sentences.secondRemodel(Context.Guild.Id) + "**" + Environment.NewLine;
                            string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                            foreach (string str in allExplanations)
                            {
                                string per = Program.getElementXml("xt\":\"", str, '"');
                                if (per != "")
                                {
                                    if (finalStr[currI].Length + per.Length > 1500)
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
                            finalStr[currI] += Environment.NewLine + "**" + Sentences.trivia(Context.Guild.Id) + "**" + Environment.NewLine;
                            string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                            foreach (string str in allExplanations)
                            {
                                string per = Program.getElementXml("xt\":\"", str, '"');
                                if (per != "")
                                {
                                    if (finalStr[currI].Length + per.Length > 1500)
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
                    await ReplyAsync(Sentences.shipgirlDontExist(Context.Guild.Id));
            }
        }
    }
}