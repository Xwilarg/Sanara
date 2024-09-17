using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl.Static
{
    class GirlsFrontline
    {
        public static void Init(IServiceProvider provider)
        {
            _tDolls = new();
            string html = provider.GetRequiredService<HttpClient>().GetStringAsync("http://iopwiki.com/wiki/T-Doll_Index").GetAwaiter().GetResult();
            html = html.Split(new[] { "Unreleased_T-Dolls_(T-Dolls_without_index_number)" }, StringSplitOptions.None)[0]; // We remove T-Dolls that weren't released
            MatchCollection match = Regex.Matches(html, "<a href=\"\\/wiki\\/([^\"]+)\" title=\"([^\"]+)\"><img");
            _tDolls = match.Cast<Match>().Select(x => (x.Groups[1].Value, x.Groups[2].Value)).ToList();
        }

        public static List<(string, string)> GetTDolls()
            => _tDolls;

        private static List<(string, string)> _tDolls;
    }
}
