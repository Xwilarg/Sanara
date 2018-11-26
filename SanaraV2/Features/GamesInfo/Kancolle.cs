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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Features.GamesInfo
{
    public class Kancolle
    {
        public static async Task<FeatureRequest<Response.DropConstruction, Error.Drop>> SearchDropConstruction(string[] args)
        {
            if (args.Length == 0)
                return (new FeatureRequest<Response.DropConstruction, Error.Drop>(null, Error.Drop.Help));
            string shipName = Utilities.CleanWord(Utilities.AddArgs(args));
            string japaneseName = GetJapaneseShipName(shipName);
            if (japaneseName == null)
                return (new FeatureRequest<Response.DropConstruction, Error.Drop>(null, Error.Drop.NotFound));
            string html;
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.Add("User-Agent", "Sanara");
                html = await hc.GetStringAsync("http://unlockacgweb.galstars.net/KanColleWiki/viewCreateShipLogList");
            }
            html = Regex.Replace(html, "\\[u]([0-9a-f]{4})",
                        m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
            MatchCollection matches = Regex.Matches(html, "\"([0-9]*)\":\"([^\"]*)\"");
            string id = null;
            string japaneseValue = "";
            foreach (char c in japaneseName)
                japaneseValue += "\\u" + ((int)c).ToString("x");
            foreach (Match match in matches)
            {
                if (match.Groups[2].Value == japaneseValue)
                {
                    id = match.Groups[1].Value;
                    break;
                }
            }
            if (id == null)
                return (new FeatureRequest<Response.DropConstruction, Error.Drop>(null, Error.Drop.NotReferenced));
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.Add("User-Agent", "Sanara");
                html = await hc.GetStringAsync("http://unlockacgweb.galstars.net/KanColleWiki/viewCreateShipLog?sid=" + id);
            }
            html = html.Split(new string[] { "order_by_probability" }, StringSplitOptions.None)[1].Split(new string[] { "flagship_order" }, StringSplitOptions.None)[0];
            int i = 0;
            List<Response.ConstructionElem> elems = new List<Response.ConstructionElem>();
            foreach (Match match in Regex.Matches(html, "{\"item1\":([0-9]+),\"item2\":([0-9]+),\"item3\":([0-9]+),\"item4\":([0-9]+),\"item5\":([0-9]+),\"sum\":[0-9]+,\"succeed\":[0-9]+,\"probability\":([0-9.]+)}"))
            {
                elems.Add(new Response.ConstructionElem()
                {
                    fuel = match.Groups[1].Value,
                    ammos = match.Groups[2].Value,
                    iron = match.Groups[3].Value,
                    bauxite = match.Groups[4].Value,
                    devMat = match.Groups[5].Value,
                    chance = match.Groups[6].Value
                });
                i++;
                if (i == 5)
                    break;
            }
            if (elems.Count == 0)
                return (new FeatureRequest<Response.DropConstruction, Error.Drop>(null, Error.Drop.DontDrop));
            return (new FeatureRequest<Response.DropConstruction, Error.Drop>(new Response.DropConstruction()
            {
                elems = elems.ToArray()
            }, Error.Drop.None));
        }

        public static async Task<FeatureRequest<Response.DropMap, Error.Drop>> SearchDropMap(string[] args)
        {
            if (args.Length == 0)
                return (new FeatureRequest<Response.DropMap, Error.Drop>(null, Error.Drop.Help));
            string shipName = Utilities.CleanWord(Utilities.AddArgs(args));
            string japaneseName = GetJapaneseShipName(shipName);
            if (japaneseName == null)
                return (new FeatureRequest<Response.DropMap, Error.Drop>(null, Error.Drop.NotFound));
            string html;
            using (HttpClient hc = new HttpClient())
                html = await hc.GetStringAsync("https://wikiwiki.jp/kancolle/%E7%AC%AC%E4%B8%80%E6%9C%9F/%E8%89%A6%E5%A8%98%E3%83%89%E3%83%AD%E3%83%83%E3%83%97%E9%80%86%E5%BC%95%E3%81%8D");
            html = html.Split(new string[] { "<tfoot>" }, StringSplitOptions.None)[1].Split(new string[] { "</tbody>" }, StringSplitOptions.None)[0];
            string[] separation = html.Split(new string[] { "<tbody>" }, StringSplitOptions.None);
            string htmlMap = separation[0];
            List<string> nodeListTmp = htmlMap.Split(new string[] { "第一期/出撃ドロップ\">" }, StringSplitOptions.None).ToList();
            nodeListTmp.RemoveAt(0);
            List<string> nodeList = new List<string>();
            foreach (string s in nodeListTmp)
                nodeList.Add(s.Substring(0, 3));
            string htmlShip = separation[1];
            string[] allDrops = htmlShip.Split(new string[] { "<tr>" }, StringSplitOptions.None);
            string shipgirl = allDrops.ToList().Find(x => x.Contains(japaneseName));
            if (shipgirl == null)
                return (new FeatureRequest<Response.DropMap, Error.Drop>(null, Error.Drop.NotReferenced));
            MatchCollection matches = Regex.Matches(shipgirl, ">([^<]*)<\\/td>");
            int index = 1;
            int i = 0;
            Dictionary<string, Response.DropMapLocation> dropMap = new Dictionary<string, Response.DropMapLocation>();
            foreach (string node in nodeList)
            {
                int nodeIndex = node[0] - '0';
                if (index != nodeIndex)
                {
                    index = nodeIndex;
                    continue;
                }
                string currVal = matches[5 + i].Groups[1].Value;
                if (currVal.Length > 0)
                {
                    switch (currVal[0])
                    {
                        case '●':
                            dropMap.Add(node, Response.DropMapLocation.NormalOnly);
                            break;

                        case '○':
                            dropMap.Add(node, Response.DropMapLocation.BossOnly);
                            break;

                        case '◎':
                            dropMap.Add(node, Response.DropMapLocation.Anywhere);
                            break;

                        default:
                            throw new NotImplementedException("SearchDropMap: Invalid character " + currVal[0]);
                    }
                }
                i++;
            }
            if (dropMap.Count == 0)
                return (new FeatureRequest<Response.DropMap, Error.Drop>(null, Error.Drop.DontDrop));
            int rating;
            int? finalRating = null;
            if (int.TryParse(matches[2].Groups[2].Value, out rating))
                finalRating = rating;
            return (new FeatureRequest<Response.DropMap, Error.Drop>(new Response.DropMap()
            {
                dropMap = dropMap,
                rarity = finalRating
            }, Error.Drop.None));
        }

        private static string GetJapaneseShipName(string englishName)
        {
            string html;
            using (HttpClient hc = new HttpClient())
                html = hc.GetStringAsync("http://kancolle.wikia.com/wiki/Internals/Translations").GetAwaiter().GetResult();
            string allShips = html.Split(new string[] { "</table>" }, StringSplitOptions.None)[0].Split(new string[] { "</th></tr>" }, StringSplitOptions.None)[1];
            MatchCollection matches = Regex.Matches(allShips, "<td>([^<]*)<\\/td><td><a href=\"\\/wiki\\/[^\"]*\" title=\"[^\"]*\"( class=\"mw-redirect\")?>([^<]*)<\\/a>");
            foreach (Match match in matches)
                if (Utilities.CleanWord(match.Groups[3].Value) == englishName)
                    return (match.Groups[1].Value);
            return (null);
        }
    }
}
