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
using BooruSharp.Booru;
using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Features.NSFW
{
    public static class Booru
    {
        private enum TagId
        {
            Safebooru,
            Gelbooru,
            Konachan,
            Rule34,
            E621,
            E926,
            Sakugabooru
        }

        private static Dictionary<string, Tuple<Type, BooruSharp.Search.Post.SearchResult>> tagInfos = new Dictionary<string, Tuple<Type, BooruSharp.Search.Post.SearchResult>>();

        public static async Task<FeatureRequest<Response.BooruSource, Error.SourceBooru>> SearchSourceBooru(string[] args)
        {
            string url = string.Join("", args);
            if (url.Length == 0)
                return new FeatureRequest<Response.BooruSource, Error.SourceBooru>(null, Error.SourceBooru.Help);
            if (!Modules.Base.Utilities.IsImage(url) || !Utilities.IsLinkValid(url))
                return new FeatureRequest<Response.BooruSource, Error.SourceBooru>(null, Error.SourceBooru.NotAnUrl);
            string html;
            using (HttpClient hc = new HttpClient())
                html = await hc.GetStringAsync("https://saucenao.com/search.php?db=999&url=" + Uri.EscapeDataString(url));
            if (!html.Contains("<div id=\"middle\">"))
                return new FeatureRequest<Response.BooruSource, Error.SourceBooru>(null, Error.SourceBooru.NotFound);
            string fullHtml = html;
            html = html.Split(new[] { "<td class=\"resulttablecontent\">" }, StringSplitOptions.None)[1];
            return new FeatureRequest<Response.BooruSource, Error.SourceBooru>(new Response.BooruSource
            {
                compatibility = float.Parse(Regex.Match(html, "<div class=\"resultsimilarityinfo\">([0-9]{2,3}\\.[0-9]{1,2})%<\\/div>").Groups[1].Value, CultureInfo.InvariantCulture),
                content = Utilities.RemoveHTML(html.Split(new[] { "<div class=\"resultcontentcolumn\">" }, StringSplitOptions.None)[1].Split(new[] { "</div>" }, StringSplitOptions.None)[0]),
                url = Regex.Match(fullHtml, "<img title=\"Index #[^\"]+\"( raw-rating=\"[^\"]+\") src=\"(https:\\/\\/img[0-9]+.saucenao.com\\/[^\"]+)\"").Groups[2].Value
            }, Error.SourceBooru.None);
        }

        // From: https://gist.github.com/Davidblkx/e12ab0bb2aff7fd8072632b396538560
        private static int GetStringDistance(string a, string b)
        {
            var source1Length = a.Length;
            var source2Length = b.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (b[j - 1] == a[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            return matrix[source1Length, source2Length];
        }

        public static async Task<FeatureRequest<Response.Booru, Error.Booru>> SearchBooru(bool isChanSafe, string[] tags, ABooru booru, Random r)
        {
            if (isChanSafe && !booru.IsSafe())
                return new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.ChanNotNSFW);
            BooruSharp.Search.Post.SearchResult res;
            List<string> newTags = null;
            try
            {
                res = await booru.GetRandomImageAsync(tags);
            }
            catch (BooruSharp.Search.InvalidTags)
            {
                newTags = new List<string>();
                foreach (string s in tags)
                {
                    string tag = s;
                    var related = await new Konachan().GetTagsAsync(s);
                    if (related.Length == 0)
                        return new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.NotFound);
                    newTags.Add(tag = related.OrderBy(x => GetStringDistance(x.name, s)).First().name);
                }
                try
                {
                    res = await booru.GetRandomImageAsync(newTags.ToArray());
                }
                catch (BooruSharp.Search.InvalidTags)
                {
                    return new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.NotFound);
                }
            }
            Error.Booru error = Error.Booru.None;
            string url = res.fileUrl.AbsoluteUri;
            Color color = GetColorFromRating(res.rating);
            string saveId = (tagInfos.Count + 1) + Utilities.GenerateRandomCode(4, r);
            tagInfos.Add(saveId, new Tuple<Type, BooruSharp.Search.Post.SearchResult>(booru.GetType(), res));
            return new FeatureRequest<Response.Booru, Error.Booru>(new Response.Booru() {
                url = url,
                colorRating = color,
                saveId = saveId,
                tags = res.tags,
                newTags = newTags?.ToArray()
            }, error);
        }

        public static async Task<FeatureRequest<Response.BooruTags, Error.BooruTags>> SearchTags(string[] idArgs)
        {
            if (idArgs.Length == 0)
                return new FeatureRequest<Response.BooruTags, Error.BooruTags>(null, Error.BooruTags.Help);
            string id = idArgs[0];
            if (!tagInfos.ContainsKey(id))
                return new FeatureRequest<Response.BooruTags, Error.BooruTags>(null, Error.BooruTags.NotFound);
            var elem = tagInfos[id];
            ABooru b = (ABooru)Activator.CreateInstance(elem.Item1, (BooruAuth)null);
            List<string> artists = new List<string>();
            List<string> sources = new List<string>();
            List<string> characs = new List<string>();
            int i = 0;
            foreach (string s in elem.Item2.tags)
            {
                i++;
                try
                {
                    switch ((await b.GetTagAsync(s)).type)
                    {
                        case BooruSharp.Search.Tag.TagType.Artist:
                            if (artists.Count == 10)
                                artists.Add("...");
                            else if (artists.Count < 10)
                                artists.Add(s);
                            break;

                        case BooruSharp.Search.Tag.TagType.Character:
                            if (characs.Count == 10)
                                characs.Add("...");
                            else if (characs.Count < 10)
                                characs.Add(s);
                            break;

                        case BooruSharp.Search.Tag.TagType.Copyright:
                            if (sources.Count == 10)
                                sources.Add("...");
                            else if (sources.Count < 10)
                                sources.Add(s);
                            break;
                    }
                }
                catch (BooruSharp.Search.InvalidTags)
                { }
            }
            uint pgcd = PGCD((uint)elem.Item2.height, (uint)elem.Item2.width);
            return new FeatureRequest<Response.BooruTags, Error.BooruTags>(new Response.BooruTags()
            {
                artistTags = artists.ToArray(),
                characTags = characs.ToArray(),
                sourceTags = sources.ToArray(),
                imageUrl = elem.Item2.previewUrl,
                rating = GetColorFromRating(elem.Item2.rating),
                booruName = elem.Item1.ToString().Split('.').Last(),
                height = elem.Item2.height,
                width = elem.Item2.width,
                aspectRatio = new Tuple<long, long>(elem.Item2.width / pgcd, elem.Item2.height / pgcd)
            }, Error.BooruTags.None);
        }

        private static uint PGCD(uint a, uint b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a == 0 ? b : a;
        }

        private static Color GetColorFromRating(BooruSharp.Search.Post.Rating rating)
        {
            switch (rating)
            {
                case BooruSharp.Search.Post.Rating.Explicit:
                    return Color.Red;

                case BooruSharp.Search.Post.Rating.Questionable:
                    return new Color(255, 255, 0);

                case BooruSharp.Search.Post.Rating.Safe:
                    return Color.Green;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
