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
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static Dictionary<string, Tuple<Type, string[]>> tagInfos = new Dictionary<string, Tuple<Type, string[]>>();

        public static async Task<FeatureRequest<Response.Booru, Error.Booru>> SearchBooru(bool isChanSafe, string[] tags, BooruSharp.Booru.Booru booru, Random r)
        {
            if (isChanSafe && !booru.IsSafe())
                return (new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.ChanNotNSFW));
            BooruSharp.Search.Post.SearchResult res;
            try
            {
                res = await booru.GetRandomImage(tags);
            }
            catch (BooruSharp.Search.InvalidTags)
            {
                return (new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.NotFound));
            }
            Error.Booru error = Error.Booru.None;
            string url = res.fileUrl.AbsoluteUri;
            if (!Utilities.IsImage(url.Split('.').Last()))
                error = Error.Booru.InvalidFile;
            Color color;
            switch (res.rating)
            {
                case BooruSharp.Search.Post.Rating.Explicit:
                    color = Color.Red;
                    break;

                case BooruSharp.Search.Post.Rating.Questionable:
                    color = new Color(255, 255, 0);
                    break;

                case BooruSharp.Search.Post.Rating.Safe:
                    color = Color.Green;
                    break;

                default:
                    throw new NotImplementedException();
            }
            string saveId = (tagInfos.Count + 1) + Utilities.GenerateRandomCode(4, r);
            tagInfos.Add(saveId, new Tuple<Type, string[]>(booru.GetType(), res.tags));
            return (new FeatureRequest<Response.Booru, Error.Booru>(new Response.Booru() {
                    url = url,
                    colorRating = color,
                    saveId = saveId,
                    tags = res.tags
                }, error));
        }

        public static async Task<FeatureRequest<Response.BooruTags, Error.BooruTags>> SearchTags(string id)
        {
            if (!tagInfos.ContainsKey(id))
                return (new FeatureRequest<Response.BooruTags, Error.BooruTags>(null, Error.BooruTags.NotFound));
            var elem = tagInfos[id];
            BooruSharp.Booru.Booru b = (BooruSharp.Booru.Booru)Activator.CreateInstance(elem.Item1);
            List<string> artists = new List<string>();
            List<string> sources = new List<string>();
            List<string> characs = new List<string>();
            int i = 0;
            foreach (string s in elem.Item2)
            {
                i++;
                switch ((await b.GetTag(s)).type)
                {
                    case BooruSharp.Search.Tag.TagType.Artist:
                        artists.Add(s);
                        break;

                    case BooruSharp.Search.Tag.TagType.Character:
                        characs.Add(s);
                        break;

                    case BooruSharp.Search.Tag.TagType.Copyright:
                        sources.Add(s);
                        break;
                }
            }
            return (new FeatureRequest<Response.BooruTags, Error.BooruTags>(new Response.BooruTags()
            {
                artistTags = artists.ToArray(),
                characTags = characs.ToArray(),
                sourceTags = sources.ToArray()
            }, Error.BooruTags.None));
        }
    }
}
