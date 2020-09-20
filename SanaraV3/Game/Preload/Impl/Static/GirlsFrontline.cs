using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SanaraV3.Game.Preload.Impl.Static
{
    class GirlsFrontline
    {
        static GirlsFrontline()
        {
            _tDolls = new List<string>();
            string html = StaticObjects.HttpClient.GetStringAsync("https://en.gfwiki.com/wiki/T-Doll_Index").GetAwaiter().GetResult();
            html = html.Split(new[] { "Unreleased_T-Dolls_(T-Dolls_without_index_number)" }, StringSplitOptions.None)[0]; // We remove T-Dolls that weren't released
            MatchCollection match = Regex.Matches(html, " < a href=\"\\/wiki\\/([^\"]+)\" title=\"[^\"]+\"><img");
            _tDolls = match.Cast<Match>().Select(x => "https://en.gfwiki.com" + x.Groups[1].Value).ToList();
        }

        public static List<string> GetTDolls()
            => _tDolls;

        private static List<string> _tDolls;
    }
}
