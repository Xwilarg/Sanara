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
            await Utilities.NotAvailable(Context.Channel as ITextChannel);
            return;
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
            Task<EmbedFieldBuilder> constructionTask = Task.Run(() => GetDropConstructionField(shipNameArr, Context.Guild.Id));
            Task<EmbedFieldBuilder> mapTask = Task.Run(() => GetDropMapField(shipNameArr, Context.Guild.Id));
            string name = string.Join("", shipNameArr);
            EmbedBuilder embed = new EmbedBuilder();
            try
            {
                embed.AddField(await mapTask);
                embed.AddField(await constructionTask);
                embed.Title = char.ToUpper(name[0]) + name.Substring(1);
                embed.Color = Color.Blue;
                await ReplyAsync("", false, embed.Build());
            }
            catch (ArgumentException ae)
            {
                await ReplyAsync(ae.Message);
            }
        }

        private async Task<EmbedFieldBuilder> GetDropConstructionField(string[] shipNameArr, ulong guildId)
        {
            string embedMsg = null;
            var result = await Features.GamesInfo.Kancolle.SearchDropConstruction(shipNameArr);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Drop.Help:
                    throw new ArgumentException(Sentences.KancolleHelp(guildId));

                case Features.GamesInfo.Error.Drop.NotFound:
                    throw new ArgumentException(Sentences.ShipgirlDontExist(guildId));

                case Features.GamesInfo.Error.Drop.NotReferenced:
                    embedMsg = Sentences.ShipNotReferencedConstruction(Context.Guild.Id);
                    break;

                case Features.GamesInfo.Error.Drop.DontDrop:
                    embedMsg = Sentences.DontDropOnMaps(Context.Guild.Id);
                    break;

                case Features.GamesInfo.Error.Drop.None:
                    int i = 1;
                    foreach (var elem in result.answer.elems)
                    {
                        embedMsg += "**" + elem.chance + "%:** "
                            + Sentences.Fuel(guildId) + " " + elem.fuel + ", "
                            + Sentences.Ammos(guildId) + " " + elem.ammos + ", "
                            + Sentences.Iron(guildId) + " " + elem.iron + ", "
                            + Sentences.Bauxite(guildId) + " " + elem.bauxite + ", "
                            + Sentences.DevMat(guildId) + " " + elem.devMat
                            + Environment.NewLine;
                        i++;
                    }
                    break;
            }
            return (new EmbedFieldBuilder()
            {
                Name = "Construction drop",
                Value = embedMsg
            });
        }

        private async Task<EmbedFieldBuilder> GetDropMapField(string[] shipNameArr, ulong guildId)
        {
            string embedMsg = null;
            var result = await Features.GamesInfo.Kancolle.SearchDropMap(shipNameArr);
            switch (result.error)
            {
                case Features.GamesInfo.Error.Drop.Help:
                    throw new ArgumentException(Sentences.KancolleHelp(guildId));

                case Features.GamesInfo.Error.Drop.NotFound:
                    throw new ArgumentException(Sentences.ShipgirlDontExist(guildId));

                case Features.GamesInfo.Error.Drop.NotReferenced:
                    embedMsg = Sentences.ShipNotReferencedMap(Context.Guild.Id);
                    break;

                case Features.GamesInfo.Error.Drop.DontDrop:
                    embedMsg = Sentences.DontDropOnMaps(Context.Guild.Id);
                    break;

                case Features.GamesInfo.Error.Drop.None:
                    foreach (var k in result.answer.dropMap)
                    {
                        embedMsg += "**" + k.Key + ":** ";
                        switch (k.Value)
                        {
                            case Features.GamesInfo.Response.DropMapLocation.NormalOnly:
                                embedMsg += Sentences.OnlyNormalNodes(Context.Guild.Id);
                                break;

                            case Features.GamesInfo.Response.DropMapLocation.BossOnly:
                                embedMsg += Sentences.OnlyBossNode(Context.Guild.Id);
                                break;

                            case Features.GamesInfo.Response.DropMapLocation.Anywhere:
                                embedMsg += Sentences.AnyNode(Context.Guild.Id);
                                break;
                        }
                        embedMsg += Environment.NewLine;
                    }
                    break;
            }
            return (new EmbedFieldBuilder()
            {
                Name = "Map drop" + ((result.answer != null && result.answer.rarity != null) ? (Sentences.Rarity(guildId)    + ": " + result.answer.rarity.ToString() + " / 7") : ("")),
                Value = embedMsg
            });
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