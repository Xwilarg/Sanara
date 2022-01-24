using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl.Static
{
    class AzurLane
    {
        static AzurLane()
        {
            _ships = new();
            string html = StaticObjects.HttpClient.GetStringAsync("https://azurlane.koumakan.jp/List_of_Ships").GetAwaiter().GetResult();
            foreach (string s in html.Split(new string[] { "title=\"Category:" }, StringSplitOptions.None))
            {
                if (s.Contains("Unreleased") || s.Contains("Plan")) // We skip ships that weren't released and were found by data mining
                    continue;
                Match match = Regex.Match(s, "\"><a href=\"\\/[^\"]+\" title=\"([^\"]+)\">(Collab|Plan)?[0-9]+<\\/a><\\/td><td><a href=\"\\/([^\"]+)\"");
                if (match.Success)
                {
                    string href = match.Groups[3].Value[5..]; // [5..] is to remove the wiki/ in front
                    if (!_ships.Any(x => x.Item1 == href)) // Some ships may appear twice because of retrofits
                        _ships.Add((href, match.Groups[1].Value));
                }
            }
        }

        public static List<(string, string)> GetShips()
            => _ships;

        private static List<(string, string)> _ships;
    }
}
