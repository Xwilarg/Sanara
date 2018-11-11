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
using Newtonsoft.Json;
using System;
using System.Net.Http;
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
            string url = "https://nhentai.net/api/galleries/" + ((noTags) ? ("all?page=0") : ("search?query=" + finalTags + "&page=8000"));
            dynamic json;
            using (HttpClient hc = new HttpClient())
            {
                json = JsonConvert.DeserializeObject(await (await hc.GetAsync(url)).Content.ReadAsStringAsync());
                int nbPages = int.Parse(json.num_pages.ToString());
                if (nbPages == 0)
                    return (new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.NotFound));
                string doujinUrl = "https://nhentai.net/api/galleries/" + ((noTags) ? ("all?") : ("search?query=" + finalTags + "&")) + "page=" + (r.Next(nbPages) + 1);
                json = JsonConvert.DeserializeObject(await (await hc.GetAsync(doujinUrl)).Content.ReadAsStringAsync());
            }
            int length = json.result.Count;
            return (new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(new Response.Doujinshi()
            {
                url = "https://nhentai.net/g/" + json.result[r.Next(0, length)].id
            }, Error.Doujinshi.None));
        }
    }
}
