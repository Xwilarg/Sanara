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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Features.NSFW
{
    public static class Doujinshi
    {
        public static async Task<FeatureRequest<Response.Doujinshi, Error.Doujinshi>> SearchDoujinshi(bool isChanSafe, string[] tags, Random r)
        {
            if (isChanSafe)
                return (new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.ChanNotNSFW));
            string finalTags = Utilities.AddArgs(tags);
            bool noTags = tags.Length == 0;
            string url;
            if (noTags)
                url = "https://nhentai.net/";
            else
                url = "https://nhentai.net/search/?q=" + Uri.EscapeDataString(finalTags);
            using (HttpClient hc = new HttpClient())
            {
                Match match = Regex.Match(await (await hc.GetAsync(url)).Content.ReadAsStringAsync(), "<a href=\"\\?(q=[^&]+&amp;)?page=([0-9]+)\" class=\"last\">");
                if (!match.Success)
                    return (new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.NotFound));
                int page = r.Next(1, int.Parse(match.Groups[2].Value) + 1);
                if (noTags)
                    url += "?page=" + page;
                else
                    url += "&page=" + page;
                MatchCollection matches = Regex.Matches(await (await hc.GetAsync(url)).Content.ReadAsStringAsync(), "<div class=\"gallery\" data-tags=\"[^\"]+\"><a href=\"\\/g\\/([0-9]+)");
                return (new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(new Response.Doujinshi()
                {
                    url = "https://nhentai.net/g/" + matches[r.Next(0, matches.Count)].Groups[1].Value
                }, Error.Doujinshi.None));
            }
        }
    }
}
