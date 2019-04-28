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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Features.Entertainment
{
    public static class Game
    {

        public static List<string> LoadBooru()
        {
            if (!File.Exists("Saves/BooruTriviaTags.dat"))
                return (null);
            List<string> tags = new List<string>();
            string[] allLines = File.ReadAllLines("Saves/BooruTriviaTags.dat");
            foreach (string line in allLines)
            {
                string[] linePart = line.Split(' ');
                if (Convert.ToInt32(linePart[1]) >= 3)
                    tags.Add(linePart[0]);
            }
            return (tags);
        }

        public static Tuple<List<string>, List<string>> LoadAnime()
        {
            if (!File.Exists("Saves/AnimeTags.dat"))
                return (null);
            List<string> tags = new List<string>();
            List<string> tagsFull = new List<string>();
            string[] allLines = File.ReadAllLines("Saves/AnimeTags.dat");
            foreach (string line in allLines)
            {
                string[] parts = line.Split(' ');
                if (int.Parse(parts[1]) > 10)
                    tags.Add(line.Split(' ')[0]);
                tagsFull.Add(line.Split(' ')[0]);
            }
            return (new Tuple<List<string>, List<string>>(tagsFull, tags));
        }

        public static async Task<List<string>> LoadAzurLane()
        {
            List<string> ships = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string json = await hc.GetStringAsync("https://azurlane.koumakan.jp/List_of_Ships");
                MatchCollection matches = Regex.Matches(json, "<a href=\"\\/[^\"]+\" title=\"([^\"]+)\">[0-9]+<\\/a>");
                foreach (Match match in matches)
                {
                    string str = match.Groups[1].Value;
                    if (!ships.Contains(str))
                        ships.Add(str);
                }
            }
            return (ships);
        }
    }
}
