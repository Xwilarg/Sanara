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
using SanaraV2.Modules.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Modules.GamesInfo
{
    [Group("Kancolle"), Alias("Kantai Collection", "KantaiCollection")]
    public class Kancolle : ModuleBase
    {
        Program p = Program.p;
        [Command("Map", RunMode = RunMode.Async), Summary("Get informations about a map")]
        public async Task Map(params string[] command)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
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
                foreach (string s in GetBranchingRules(infos))
                    if (s != "")
                        await ReplyAsync(Regex.Replace(s, "<!---(?s:.)*--->", ""));
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
        public static string[] GetBranchingRules(string infos)
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
            string finalInfos = "";
            foreach (string s in infos.Split(new string[] { "\n===" }, StringSplitOptions.None).Skip(1))
                finalInfos += "**" + s.Replace("*", "\\*").Replace("===", "**").Replace("{{Map/Footer}}", "") + Environment.NewLine;
            return (new string[] { finalStr, finalInfos });
        }

        [Command("Drop", RunMode = RunMode.Async), Summary("Get informations about a drop")]
        public async Task Drop(params string[] shipNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
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
                        return (Sentences.Rarity(guildId) + ": " + ((rarity.Length > 0) ? (rarity) : ("?")) + "/7" + Environment.NewLine + finalStr);
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

        [Command("", RunMode = RunMode.Async), Priority(-1)]
        public async Task CharacDefault(params string[] shipNameArr) => await Charac(shipNameArr);

        [Command("Charac", RunMode = RunMode.Async), Summary("Get informations about a Kancolle character")]
        public async Task Charac(params string[] shipNameArr)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Kancolle);
            if (shipNameArr.Length == 0)
            {
                await ReplyAsync(Sentences.KancolleHelp(Context.Guild.Id));
                return;
            }
            string shipName = Utilities.AddArgs(shipNameArr);
            IGuildUser me = await Context.Guild.GetUserAsync(Base.Sentences.myId);
            Wikia.CharacInfo? infos = Wikia.GetCharacInfos(shipName, Wikia.WikiaType.KanColle);
            if (infos == null)
            {
                await ReplyAsync(Sentences.ShipgirlDontExist(Context.Guild.Id));
                return;
            }
            string thumbnailPath = null;
            List<string> finalStr = FillKancolleInfos(infos.Value.id, Context.Guild.Id);
            if (me.GuildPermissions.AttachFiles)
            {
                thumbnailPath = Wikia.DownloadCharacThumbnail(infos.Value.thumbnail);
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
            return (Wikia.FillWikiaInfos(shipId, guildId, new Dictionary<string, Func<ulong, string>>
            {
                { "Personality", Sentences.Personality },
                { "Appearance", Sentences.Appearance },
                { "Second Remodel", Sentences.SecondRemodel },
                { "Trivia", Sentences.Trivia },
                { "Libeccio CG", Sentences.LibeccioCG },
                { "Libeccio as a ship", Sentences.LibeccioAsAShip },
                { "In-game", Sentences.InGame },
                { "Historical", Sentences.Historical }
            }, Wikia.WikiaType.KanColle));
        }
    }
}