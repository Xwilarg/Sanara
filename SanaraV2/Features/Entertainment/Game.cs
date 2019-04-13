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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SanaraV2.Features.Entertainment.Response;

namespace SanaraV2.Features.Entertainment
{
    public static class Game
    {
        public static async Task<FeatureRequest<Response.Score, Error.Score>> Score(Db.Db db, ulong guildId, IReadOnlyCollection<IGuildUser> users)
        {
            var res = await db.GetAllScores();
            var me = res[guildId.ToString()];
            if (me == null)
                return (new FeatureRequest<Response.Score, Error.Score>(null, Error.Score.NoScore));
            string[] games = new string[] { "shiritori", "anime", "booru", "kancolle" };
            Response.Score.ScoreItem shiritori = null, anime = null, booru = null, kancolle = null;
            int i = -1;
            foreach (var elem in games)
            {
                i++;
                if (!me.ContainsKey(elem))
                    continue;
                string[] myElems = me[elem].Split('|');
                Response.Score.ScoreItem item = new Score.ScoreItem();
                item.myScore = int.Parse(myElems[0]);
                item.contributors = myElems.Skip(1).Select(x => users.Where(y => y.Id.ToString() == x).ElementAt(0).ToString()).ToArray();
                item.rankedNumber = res.Where(x => x.Value.ContainsKey(elem)).Count();
                item.myRanking = res.Where(x => x.Value.ContainsKey(elem) && int.Parse(x.Value[elem].Split('|')[0]) > item.myScore).Count() + 1;
                if (i == 0) shiritori = item;
                else if (i == 1) anime = item;
                else if (i == 2) booru = item;
                else if (i == 3) kancolle = item;
            }
            return (new FeatureRequest<Response.Score, Error.Score>(new Response.Score()
            {
                anime = anime,
                booru = booru,
                kancolle = kancolle,
                shiritori = shiritori
            }, Error.Score.None));
        }

        public static async Task<FeatureRequest<Response.Game, Error.Game>> Play(string[] args, bool isChanNsfw, ulong chanId, List<Modules.Entertainment.GameModule.Game> games)
        {
            if (games.Any(x => x.m_chan.Id == chanId))
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.AlreadyRunning));
            if (args.Length == 0)
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.WrongName));
            string gameName = args[0].ToLower();
            bool isNormal = true;
            if (args.Length > 1)
            {
                string difficulty = args[1].ToLower();
                if (difficulty == "easy")
                    isNormal = false;
                else if (difficulty != "normal")
                    return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.WrongDifficulty));
            }
            GameName gn;
            if (gameName == "shiritori")
                gn = GameName.Shiritori;
            else if (gameName == "anime")
                gn = GameName.Anime;
            else if (gameName == "booru" || gameName == "gelbooru")
                gn = GameName.Booru;
            else if (gameName == "kancolle" || gameName == "kantaicollection" || gameName == "kc")
                gn = GameName.Kancolle;
            else if (gameName == "fireemblem" || gameName == "fe")
                gn = GameName.FireEmblem;
            else
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.WrongName));
            if (gn == GameName.Booru && !isChanNsfw)
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.NotNsfw));
            return (new FeatureRequest<Response.Game, Error.Game>(new Response.Game()
            {
                gameName = gn,
                isNormal = isNormal
            }, Error.Game.None));
        }

        public static List<string> LoadShiritori()
        {
            if (!File.Exists("Saves/shiritoriWords.dat"))
                return (null);
            return (File.ReadAllLines("Saves/shiritoriWords.dat").ToList());
        }

        public static async Task<List<string>> LoadKancolle()
        {
            List<string> ships = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string json = await hc.GetStringAsync("http://kancolle.wikia.com/wiki/Ship?action=raw");
                json = json.Split(new string[] { "List of destroyers" }, StringSplitOptions.None)[1].Split(new string[] { "=={{anchor|type}}" }, StringSplitOptions.None)[0];
                MatchCollection matches = Regex.Matches(json, @"\[\[([^]]+)\]\]");
                foreach (Match match in matches)
                {
                    string str = match.Groups[1].Value.Replace("'", "").Split('|')[0];
                    if (!str.StartsWith("List of") && !str.StartsWith("Auxiliary") && !ships.Contains(str))
                        ships.Add(str);
                }
            }
            return (ships);
        }

        public static List<string> LoadBooru()
        {
            if (!File.Exists("Saves/BooruTriviaTags.dat"))
                return (null);
            List<string> tags = new List<string>();
            string[] allLines = File.ReadAllLines("Saves/BooruTriviaTags.dat");
            foreach (string line in allLines)
            {
                string[] linePart = line.Split(' ');
                if (Convert.ToInt32(linePart[1]) > 3)
                    tags.Add(linePart[0]);
            }
            return (tags);
        }

        public static List<string> LoadAnime()
        {
            if (!File.Exists("Saves/AnimeTags.dat"))
                return (null);
            List<string> tags = new List<string>();
            string[] allLines = File.ReadAllLines("Saves/AnimeTags.dat");
            foreach (string line in allLines)
            {
                string[] parts = line.Split(' ');
                if (int.Parse(parts[1]) > 10)
                    tags.Add(line.Split(' ')[0]);
            }
            return (tags);
        }

        public static async Task<List<Tuple<string, string, string>>> LoadFireEmblem() // Not working
        {
            List<Tuple<string, string, string>> characters = new List<Tuple<string, string, string>>();
            using (HttpClient hc = new HttpClient())
            {
                string json = await hc.GetStringAsync("https://feheroes.gamepedia.com/Hero_list");
                json = json.Split(new string[] { "<table" }, StringSplitOptions.None)[1].Split(new string[] { "</table>" }, StringSplitOptions.None)[0];
                characters = new List<Tuple<string, string, string>>();
                MatchCollection matches = Regex.Matches(json, "title=\"[^\"]+\">([^<]+)<\\/a><\\/td><td[^>]*>([^<]+<\\/td>)");
                foreach (Match match in matches)
                {
                    try
                    {
                        string name = match.Groups[1].Value.Split(':')[0];
                        if (!characters.Any(x => x.Item1 == name))
                        {
                            dynamic dyn = JsonConvert.DeserializeObject(await hc.GetStringAsync("https://fireemblem.fandom.com/api/v1/Search/List?query=" + name + "&limit=1"));
                            string id = dyn.items[0].id;
                            dyn = JsonConvert.DeserializeObject(await hc.GetStringAsync("http://fireemblem.wikia.com/api/v1/Articles/Details?ids=" + id));
                            string thumbnailUrl = dyn.items[id].thumbnail;
                            if (thumbnailUrl == null)
                                continue;
                            thumbnailUrl = thumbnailUrl.Split(new string[] { "/revision" }, StringSplitOptions.None)[0];
                            characters.Add(new Tuple<string, string, string>(name, thumbnailUrl, match.Groups[2].Value));
                        }
                    } catch (HttpRequestException)
                    { }
                }
            }
            return (characters);
        }
    }
}
