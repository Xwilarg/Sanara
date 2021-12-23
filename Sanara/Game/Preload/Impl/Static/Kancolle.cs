using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl.Static
{
    public static class Kancolle
    {
        static Kancolle()
        {
            _ships = new();
            string json = StaticObjects.HttpClient.GetStringAsync("https://kancolle.fandom.com/wiki/Ship").GetAwaiter().GetResult();
            // We get the first table and remove everything before coastal defense ships (headers and stuffs)
            // We also remove the fleet of fog (event from 2013, people couldn't keep the ships after it)
            json = json.Split(new string[] { "List_of_coastal_defense_ships_by_upgraded_maximum_stats" }, StringSplitOptions.None)[1].Split(new string[] { "Fleet_of_Fog" }, StringSplitOptions.None)[0];
            MatchCollection matches = Regex.Matches(json, "<a href=\"\\/wiki\\/([^\"]+)\" title=\"[^\"]+\">[^<]+<\\/a>");
            foreach (Match match in matches)
            {
                string str = match.Groups[1].Value;
                if (!str.StartsWith("List_of") && !str.StartsWith("Category:"))
                    _ships.Add(str);
            }
        }

        public static List<string> GetShips()
            => _ships;

        private static List<string> _ships;
    }
}
