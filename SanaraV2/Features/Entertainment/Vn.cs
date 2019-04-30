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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VndbSharp;
using VndbSharp.Models;
using VndbSharp.Models.VisualNovel;

namespace SanaraV2.Features.Entertainment
{
    public class Vn
    {
        public static async Task<FeatureRequest<Response.Vn, Error.Vn>> SearchVn(string[] args, bool isChanSfw)
        {
            if (args.Length == 0)
                return (new FeatureRequest<Response.Vn, Error.Vn>(null, Error.Vn.Help));
            string vnName = string.Join("", args);
            string cleanVnName = Utilities.CleanWord(vnName);
            Vndb client = new Vndb();
            uint id = 0;
            string html;
            // HttpClient doesn't really look likes to handle redirection properly
            HttpWebRequest http = (HttpWebRequest)WebRequest.Create("https://vndb.org/v/all?sq=" + vnName.Replace(' ', '+'));
            http.AllowAutoRedirect = false;
            using (HttpWebResponse response = (HttpWebResponse)http.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            // Parse HTML and go though every VN, check the original name and translated name to get the VN id
            MatchCollection matches = Regex.Matches(html, "<a href=\"\\/v([0-9]+)\" title=\"([^\"]+)\">([^<]+)<\\/a>");
            foreach (Match match in matches)
            {
                if (Utilities.CleanWord(match.Groups[3].Value).Contains(cleanVnName)
                    || match.Groups[2].Value.Contains(vnName))
                {
                    id = uint.Parse(match.Groups[1].Value);
                    break;
                }
            }
            // If no matching name, we take the first one in the search list, if none these NotFound
            if (id == 0)
            {
                if (matches.Count == 0)
                    return (new FeatureRequest<Response.Vn, Error.Vn>(null, Error.Vn.NotFound));
                else
                    id = uint.Parse(matches[0].Groups[1].Value);
            }
            VisualNovel vn = (await client.GetVisualNovelAsync(VndbFilters.Id.Equals(id), VndbFlags.FullVisualNovel)).ToArray()[0];
            return (new FeatureRequest<Response.Vn, Error.Vn>(new Response.Vn()
            {
                originalTitle = vn.OriginalName,
                title = vn.Name,
                imageUrl = (isChanSfw && vn.IsImageNsfw) ? null : vn.Image,
                description = Utilities.RemoveExcess(vn.Description != null ? string.Join("\n", vn.Description.Split('\n').Where(x => !x.Contains("[/url]"))) : null),
                isAvailableEnglish = vn.Languages.Contains("en"),
                isAvailableWindows = vn.Platforms.Contains("win"),
                rating = vn.Rating,
                releaseYear = vn.Released.Year,
                releaseMonth = vn.Released.Month,
                releaseDay = vn.Released.Day,
                length = vn.Length
            }, Error.Vn.None));
        }
    }
}
