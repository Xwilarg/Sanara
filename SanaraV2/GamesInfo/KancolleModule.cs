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
        public async Task Map(params string[] command)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            if (IsMapInvalid(command))
                await ReplyAsync(Sentences.MapHelp(Context.Guild.Id));
            else
            {
                string mapIntro, mapDraw, mapName;
                string infos = GetMapInfos(command[0][0], command[1][0], out mapIntro, out mapDraw, out mapName);
                await ReplyAsync(mapName);
                await Context.Channel.SendFileAsync(mapIntro);
                await Context.Channel.SendFileAsync(mapDraw);
                File.Delete(mapIntro);
                File.Delete(mapDraw);
                await ReplyAsync(GetBranchingRules(infos));
            }
        }

        /// <summary>
        /// Return if a map is valid or not
        /// </summary>
        /// <param name="mapName">Array containing world (in first string) and level (in the second one)</param>
        public static bool IsMapInvalid(string[] mapName)
        {
            return (mapName.Length != 2 || mapName[0].Length != 1 || mapName[1].Length != 1
                || mapName[0][0] <= '0' || mapName[0][0] > '6' || mapName[1][0] <= '0' || mapName[1][0] > '6'
                || (mapName[0][0] != '1' && mapName[1][0] == '6'));
        }

        /// <summary>
        /// Get informations about a map
        /// </summary>
        /// <param name="world">The world the map belong in</param>
        /// <param name="level">The level the map belong in</param>
        /// <param name="mapIntro">Return the image of the map</param>
        /// <param name="mapDraw">Return the image of the branching of the map</param>
        /// <param name="mapName">Return the name of the map</param>
        /// <returns>Return the informations about the map</returns>
        public static string GetMapInfos(char world, char level, out string mapIntro, out string mapDraw, out string mapName)
        {
            using (WebClient wc = new WebClient())
            {

                string url = "http://kancolle.wikia.com/wiki/World_" + world + "/" + world + "-" + level;
                string html = wc.DownloadString(url);
                wc.Encoding = Encoding.UTF8;
                string htmlRaw = wc.DownloadString(url + "?action=raw");
                html = html.Split(new string[] { "typography-xl-optout" }, StringSplitOptions.None)[1];
                string[] allLinks = html.Split(new string[] { "href=" }, StringSplitOptions.None);
                int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                wc.DownloadFile(Utilities.GetElementXml("\"", allLinks[1], '"'), "kancolleMap" + currentTime + "1.png");
                mapIntro = "kancolleMap" + currentTime + "1.png";
                wc.DownloadFile(Utilities.GetElementXml("\"", allLinks[2], '"'), "kancolleMap" + currentTime + "2.png");
                mapDraw = "kancolleMap" + currentTime + "2.png";
                mapName = Utilities.GetElementXml("|en = ", htmlRaw, '\n');
                return (htmlRaw);
            }
        }

        /// <summary>
        /// Return informations about the branching rule of the map
        /// </summary>
        /// <param name="infos">string containing informations to parse</param>
        public static string GetBranchingRules(string infos)
        {
            string branchingRules;
            if (infos.Contains("{{MapBranchingTable"))
                branchingRules = infos.Split(new string[] { "{{MapBranchingTable" }, StringSplitOptions.None)[1];
            else
                branchingRules = infos.Split(new string[] { "{{Map/Branching" }, StringSplitOptions.None)[1];
            string[] allBranches = branchingRules.Split(new string[] { "}}" }, StringSplitOptions.None)[0].Split('\n');
            string finalStr = "";
            foreach (string currBranch in allBranches)
            {
                if (currBranch.Length == 0 || currBranch.StartsWith("|title") || currBranch.StartsWith("|id"))
                    continue;
                string line = currBranch.Substring(1, currBranch.Length - 1);
                finalStr += line + Environment.NewLine;
            }
            return (finalStr);
        }

        [Command("Drop", RunMode = RunMode.Async), Summary("Get informations about a drop")]
        public async Task Drop(params string[] shipNameArr)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            if (shipNameArr.Length == 0)
            {
                await ReplyAsync(Sentences.KancolleHelp(Context.Guild.Id));
                return;
            }
            string shipName = GetShipName(shipNameArr);
            if (shipName == null)
            {
                await ReplyAsync(Sentences.ShipgirlDontExist(Context.Guild.Id));
                return;
            }
            DropMapError error;
            string dropMap = GetDropMap(shipName, Context.Guild.Id, out error);
            if (error == DropMapError.DontDrop)
                await ReplyAsync(Sentences.DontDropOnMaps(Context.Guild.Id));
            else if (error == DropMapError.NotReferenced)
                await ReplyAsync(Sentences.ShipNotReferencedMap(Context.Guild.Id));
            else
                await ReplyAsync(dropMap);
            string dropConstruction = GetDropConstruction(shipName, Context.Guild.Id);
            if (dropConstruction == null)
                await ReplyAsync(Sentences.ShipNotReferencedConstruction(Context.Guild.Id));
            else
                await ReplyAsync(dropConstruction);
        }

        /// <summary>
        /// Return ship name in kanji
        /// </summary>
        /// <param name="shipNameArr">Ship name in romaji</param>
        public static string GetShipName(string[] shipNameArr)
        {
            string shipName = Utilities.CleanWord(Utilities.AddArgs(shipNameArr));
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                string html = wc.DownloadString("http://kancolle.wikia.com/wiki/Internals/Translations");
                List<string> allShipsName = html.Split(new string[] { "<tr" }, StringSplitOptions.None).ToList();
                allShipsName.RemoveAt(0);
                string shipContain = allShipsName.Find(x => Utilities.CleanWord(Utilities.GetElementXml("\">", x, '<')) == shipName);
                if (shipContain == null)
                    return (null);
                return (Utilities.GetElementXml("<td>", shipContain, '<'));
            }
        }

        public enum DropMapError
        {
            NoError,
            NotReferenced,
            DontDrop
        }

        /// <summary>
        /// Return the drop on maps for a ship
        /// </summary>
        /// <param name="shipName">Name of the ship</param>
        /// <param name="guildId">Guild ID was sent from</param>
        /// <param name="error">If the research fail, return the error in this param</param>
        public static string GetDropMap(string shipName, ulong guildId, out DropMapError error)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                string html = wc.DownloadString("https://wikiwiki.jp/kancolle/%E8%89%A6%E5%A8%98%E3%83%89%E3%83%AD%E3%83%83%E3%83%97%E9%80%86%E5%BC%95%E3%81%8D");
                html = html.Split(new string[] { "<thead>" }, StringSplitOptions.None)[1];
                html = html.Split(new string[] { "艦種別表" }, StringSplitOptions.None)[0];
                string[] shipgirls = html.Split(new string[] { "<tr>" }, StringSplitOptions.None);
                string shipgirl = shipgirls.ToList().Find(x => x.Contains(shipName));
                string finalStr = "";
                if (shipgirl == null)
                {
                    error = DropMapError.NotReferenced;
                    return (null);
                }
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
                        string node = Utilities.GetElementXml(">", cathegories[i], '<');
                        if (node.Length > 0)
                        {
                            switch (node[0])
                            {
                                case '●':
                                    finalStr += world + "-" + level + ": " + Sentences.OnlyNormalNodes(guildId) + Environment.NewLine;
                                    break;

                                case '○':
                                    finalStr += world + "-" + level + ": " + Sentences.OnlyBossNode(guildId) + Environment.NewLine;
                                    break;

                                case '◎':
                                    finalStr += world + "-" + level + ": " + Sentences.AnyNode(guildId) + Environment.NewLine;
                                    break;

                                default:
                                    finalStr += world + "-" + level + ": " + Sentences.DefaultNode(guildId) + Environment.NewLine;
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
                        string rarity = Utilities.GetElementXml(">", cathegories[1], '<');
                        error = DropMapError.NoError;
                        return (Sentences.Rarity(guildId) + " " + ((rarity.Length > 0) ? (rarity) : ("?")) + "/7" + Environment.NewLine + finalStr);
                    }
                    error = DropMapError.DontDrop;
                    return (null);
                }
            }
        }

        /// <summary>
        /// Return the drop on construction for a ship
        /// </summary>
        /// <param name="shipName">Name of the ship</param>
        /// <param name="guildId">Guild ID the message was sent from</param>
        public static string GetDropConstruction(string shipName, ulong guildId) // shipName in kanji
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("User-Agent: Sanara");
                string html = wc.DownloadString("http://unlockacgweb.galstars.net/KanColleWiki/viewCreateShipLogList");
                html = Regex.Replace(
                        html,
                        @"\\[Uu]([0-9A-Fa-f]{4})",
                        m => char.ToString(
                            (char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier))); // Replace \\u1313 by \u1313
                string[] htmlSplit = html.Split(new string[] { shipName }, StringSplitOptions.None);
                if (htmlSplit.Length == 1)
                    return (null);
                string[] allIds = htmlSplit[htmlSplit.Length - 2].Split(new string[] { "\",\"" }, StringSplitOptions.None);
                wc.Headers.Add("User-Agent: Sanara");
                html = wc.DownloadString("http://unlockacgweb.galstars.net/KanColleWiki/viewCreateShipLog?sid=" + (allIds[allIds.Length - 1].Split('"')[0]));
                html = html.Split(new string[] { "order_by_probability" }, StringSplitOptions.None)[1];
                html = html.Split(new string[] { "flagship_order" }, StringSplitOptions.None)[0];
                string[] allElements = html.Split(new string[] { "{\"item1\":" }, StringSplitOptions.None);
                string finalStr = Sentences.ShipConstruction(guildId) + Environment.NewLine;
                for (int i = 1; i < ((allElements.Length > 6) ? (6) : (allElements.Length)); i++)
                {
                    string[] ressources = allElements[i].Split(new string[] { ",\"" }, StringSplitOptions.None);
                    finalStr += Utilities.GetElementXml(":", ressources[7], '}') + "% -- " + Sentences.Fuel(guildId) + " " + ressources[0] + ", " + Sentences.Ammos(guildId) + " " + Utilities.GetElementXml(":", ressources[1], '?') + ", " + Sentences.Iron(guildId) + " " + Utilities.GetElementXml(":", ressources[2], '?') + ", " + Sentences.Bauxite(guildId) + " " + Utilities.GetElementXml(":", ressources[3], '?')
                         + ", " + Sentences.DevMat(guildId) + " " + Utilities.GetElementXml(":", ressources[4], '?') + Environment.NewLine;
                }
                return (finalStr);
            }
        }

        [Command("Kancolle", RunMode = RunMode.Async), Summary("Get informations about a Kancolle character")]
        public async Task Charac(params string[] shipNameArr)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            if (shipNameArr.Length == 0)
            {
                await ReplyAsync(Sentences.KancolleHelp(Context.Guild.Id));
                return;
            }
            string shipName = Utilities.AddArgs(shipNameArr);
            IGuildUser me = await Context.Guild.GetUserAsync(Sentences.myId);
            string id, thumbnail;
            if (!GetShipInfos(shipName, out id, out thumbnail))
            {
                await ReplyAsync(Sentences.ShipgirlDontExist(Context.Guild.Id));
                return;
            }
            string thumbnailPath = null;
            List<string> finalStr = FillKancolleInfos(id, Context.Guild.Id);
            if (me.GuildPermissions.AttachFiles)
            {
                thumbnailPath = DownloadShipThumbnail(thumbnail);
                await Context.Channel.SendFileAsync(thumbnailPath);
            }
            foreach (string s in finalStr)
                await ReplyAsync(s);
            if (me.GuildPermissions.AttachFiles)
                File.Delete(thumbnailPath);
        }

        /// <summary>
        /// Return informations about a ship
        /// </summary>
        /// <param name="shipId">ID of the ship</param>
        public static List<string> FillKancolleInfos(string shipId, ulong guildId)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                List<string> finalStr = new List<string>() {
                    ""
                };
                string url = "http://kancolle.wikia.com/api/v1/Articles/AsSimpleJson?id=" + shipId;
                string json = wc.DownloadString(url);
                string[] jsonInside = json.Split(new string[] { "\"title\"" }, StringSplitOptions.None);
                int currI = 0;
                GetKancolleInfo("Personality", ref currI, ref finalStr, jsonInside, Sentences.Personality(guildId));
                GetKancolleInfo("Appearance", ref currI, ref finalStr, jsonInside, Sentences.Appearance(guildId));
                GetKancolleInfo("Second Remodel", ref currI, ref finalStr, jsonInside, Sentences.SecondRemodel(guildId));
                GetKancolleInfo("Trivia", ref currI, ref finalStr, jsonInside, Sentences.Trivia(guildId));
                GetKancolleInfo("In-game", ref currI, ref finalStr, jsonInside, Sentences.InGame(guildId));
                GetKancolleInfo("Historical", ref currI, ref finalStr, jsonInside, Sentences.Historical(guildId));
                return (finalStr.Select(x => Regex.Replace(x, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)))).ToList());
            }
        }

        /// <summary>
        /// Download ship thumbnail given an URL
        /// </summary>
        /// <param name="fullUrl">The cropped URL given byGetShipInfos</param>
        /// <returns>The path to the file downloaded</returns>
        public static string DownloadShipThumbnail(string fullUrl)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                string shipName = "shipgirl" + currentTime + ".jpg";
                wc.DownloadFile(fullUrl, shipName);
                return (shipName);
            }
        }

        /// <summary>
        /// Return informations about a ship
        /// </summary>
        /// <param name="shipName">Name of the ship</param>
        /// <param name="id">Return the id of the ship on KanColle wikia</param>
        /// <param name="thumbnail">Return the thumbnail of the ship</param>
        public static bool GetShipInfos(string shipName, out string id, out string thumbnail)
        {
            try
            {
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    List<string> finalStr = new List<string> { "" };
                    string json = w.DownloadString("https://kancolle.wikia.com/api/v1/Search/List?query=" + shipName + "&limit=1");
                    id = Utilities.GetElementXml("\"id\":", json, ',');
                    string title = Utilities.GetElementXml("\"title\":\"", json, '"');
                    json = w.DownloadString("http://kancolle.wikia.com/api/v1/Articles/Details?ids=" + id);
                    thumbnail = Utilities.GetElementXml("\"thumbnail\":\"", json, '"');
                    thumbnail = thumbnail.Split(new string[] { ".jpg" }, StringSplitOptions.None)[0] + ".jpg";
                    thumbnail = thumbnail.Replace("\\", "");
                    if (title.ToUpper() != shipName.ToUpper())
                        return (false);
                    string url = "http://kancolle.wikia.com/wiki/" + title + "?action=raw";
                    json = w.DownloadString(url);
                    if (Utilities.GetElementXml("{{", json, '}') != "ShipPageHeader" && Utilities.GetElementXml("{{", json, '}') != "Ship/Header")
                        return (false);
                    return (true);
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse code = ex.Response as HttpWebResponse;
                id = null;
                thumbnail = null;
                if (code.StatusCode == HttpStatusCode.NotFound)
                    return (false);
                else
                    throw ex;
            }
        }

        /// <summary>
        /// Return  informations about a ship on a categorie from KanColle wikia
        /// </summary>
        /// <param name="categorie">Name of the categorie on the JSON</param>
        /// <param name="currI">Counter for finalStr</param>
        /// <param name="finalStr">List containing the informations</param>
        /// <param name="jsonInside">Informations to parse</param>
        /// <param name="relatedSentence">Name of the categorie to display</param>
        private static void GetKancolleInfo(string categorie, ref int currI, ref List<string> finalStr, string[] jsonInside, string relatedSentence)
        {
            foreach (string s in jsonInside)
            {
                if (s.Contains(categorie))
                {
                    finalStr[currI] += Environment.NewLine + "**" + relatedSentence + "**" + Environment.NewLine;
                    string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                    foreach (string str in allExplanations)
                    {
                        string per = Utilities.GetElementXml("xt\":\"", str, '"');
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
        }
    }
}