using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SanaraV3.Game.Preload.Impl.Static
{
    class AzurLane
    {
        static AzurLane()
        {
            _ships = new List<(string, string)>();
            string html = StaticObjects.HttpClient.GetStringAsync("https://azurlane.koumakan.jp/List_of_Ships").GetAwaiter().GetResult();
            foreach (string s in html.Split(new string[] { "title=\"Category:" }, StringSplitOptions.None))
            {
                if (s.Contains("Unreleased") || s.Contains("Plan")) // We skip ships that weren't released and were found by data mining
                    continue;
                Match match = Regex.Match(s, "\"><a href=\"\\/[^\"]+\" title=\"[^\"]+\">(Collab|Plan)?[0-9]+<\\/a><\\/td><td><a href=\"\\/([^\"]+)\"");
                if (match.Success)
                {
                    string str = match.Groups[2].Value;
                   // if (!ships.Contains(str)) // Some ships may appear twice because of retrofits
                   //     ships.Add(str);
                }
            }
        }

        public static List<(string, string)> GetShips()
            => _ships;

        private static List<(string, string)> _ships;
    }
}
