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
using NHentaiSharp.Core;
using NHentaiSharp.Exception;
using NHentaiSharp.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV2.Features.NSFW
{
    public static class Doujinshi
    {
        public static async Task<FeatureRequest<Response.Doujinshi, Error.Doujinshi>> SearchAdultVideo(bool isChanSafe, string[] tags, Random r)
        {
            if (isChanSafe)
                return new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.ChanNotNSFW);
            return new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.None);
        }

        public static async Task<FeatureRequest<Response.Doujinshi, Error.Doujinshi>> SearchCosplay(bool isChanSafe, string[] tags, Random r)
        {
            if (isChanSafe)
                return new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.ChanNotNSFW);
            string html;
            string url = "https://e-hentai.org/?f_cats=959&f_search=" + Uri.EscapeDataString(string.Join(" ", tags));
            int randomDoujinshi;
            string imageUrl;
            List<string> allTags = new List<string>();
            string finalUrl;
            using (HttpClient hc = new HttpClient())
            {
                html = await hc.GetStringAsync(url);
                Match m = Regex.Match(html, "Showing ([0-9,]+) result");
                if (!m.Success)
                    return new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.NotFound);
                randomDoujinshi = r.Next(0, int.Parse(m.Groups[1].Value.Replace(",", "")));
                html = await hc.GetStringAsync(url + "&page=" + randomDoujinshi / 25);
                finalUrl = Regex.Matches(html, "<a href=\"(https:\\/\\/e-hentai\\.org\\/g\\/[^\"]+)\"")[randomDoujinshi % 25].Groups[1].Value;
                html = await hc.GetStringAsync(finalUrl);

                string htmlTags = html.Split(new[] { "taglist" }, StringSplitOptions.None)[1].Split(new[] { "Showing" }, StringSplitOptions.None)[0];
                foreach (Match match in Regex.Matches(htmlTags, ">([^<]+)<\\/a><\\/div>"))
                    allTags.Add(match.Groups[1].Value);

                // To get the cover image, we first must go the first image of the gallery then we get it
                string htmlCover = await hc.GetStringAsync(Regex.Match(html, "<a href=\"([^\"]+)\"><img alt=\"0*1\"").Groups[1].Value);
                imageUrl = Regex.Match(htmlCover, "<img id=\"img\" src=\"([^\"]+)\"").Groups[1].Value;
            }
            return new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(new Response.Doujinshi()
            {
                url = finalUrl,
                imageUrl = imageUrl,
                title = HttpUtility.HtmlDecode(Regex.Match(html, "<title>(.+) - E-Hentai Galleries<\\/title>").Groups[1].Value),
                tags = allTags.ToArray()
            }, Error.Doujinshi.None);
        }

        public static async Task<FeatureRequest<Response.Doujinshi, Error.Doujinshi>> SearchDoujinshi(bool isChanSafe, string[] tags, Random r)
        {
            if (isChanSafe)
                return new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.ChanNotNSFW);
            SearchResult result;
            try
            {
                result = await (tags.Length == 0 ? SearchClient.SearchAsync() : SearchClient.SearchWithTagsAsync(tags));
            } catch (InvalidArgumentException)
            {
                return new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(null, Error.Doujinshi.NotFound);
            }
            int page = r.Next(0, result.numPages) + 1;
            result = await (tags.Length == 0 ? SearchClient.SearchAsync(page) : SearchClient.SearchWithTagsAsync(tags, page));
            var doujinshi = result.elements[r.Next(0, result.elements.Length)];
            return new FeatureRequest<Response.Doujinshi, Error.Doujinshi>(new Response.Doujinshi()
            {
                url = doujinshi.url.AbsoluteUri,
                imageUrl = doujinshi.pages[0].imageUrl.AbsoluteUri,
                title = doujinshi.prettyTitle,
                tags = doujinshi.tags.Select(x => x.name).ToArray()
            }, Error.Doujinshi.None);
        }
    }
}
