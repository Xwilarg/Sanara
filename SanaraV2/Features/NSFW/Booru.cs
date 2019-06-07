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

        private static Dictionary<string, Tuple<Type, BooruSharp.Search.Post.SearchResult>> tagInfos = new Dictionary<string, Tuple<Type, BooruSharp.Search.Post.SearchResult>>();

        public static async Task<FeatureRequest<Response.Booru, Error.Booru>> SearchBooru(bool isChanSafe, string[] tags, BooruSharp.Booru.Booru booru, Random r)
        {
            if (isChanSafe && !booru.IsSafe())
                return new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.ChanNotNSFW);
            BooruSharp.Search.Post.SearchResult res;
            try
            {
                res = await booru.GetRandomImage(tags);
            }
            catch (BooruSharp.Search.InvalidTags)
            {
                List<string> newTags = new List<string>();
                foreach (string s in tags)
                {
                    string tag = s;
                    if ((await booru.GetNbImage(s)) == 0)
                    {
                        var related = await new BooruSharp.Booru.Konachan().GetTags(s);
                        tag = null;
                        foreach (var rTag in related)
                            if ((await booru.GetNbImage(rTag.name)) > 0)
                            {
                                tag = rTag.name;
                                break;
                            }
                        if (tag == null)
                            return (new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.NotFound));
                    }
                    newTags.Add(tag);
                }
                try
                {
                    res = await booru.GetRandomImage(newTags.ToArray());
                }
                catch (BooruSharp.Search.InvalidTags)
                {
                    return new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.NotFound);
                }
            }
            Error.Booru error = Error.Booru.None;
            string url = res.fileUrl.AbsoluteUri;
            if (!Utilities.IsImage(url.Split('.').Last()))
                error = Error.Booru.InvalidFile;
            Color color = GetColorFromRating(res.rating);
            string saveId = (tagInfos.Count + 1) + Utilities.GenerateRandomCode(4, r);
            tagInfos.Add(saveId, new Tuple<Type, BooruSharp.Search.Post.SearchResult>(booru.GetType(), res));
            return new FeatureRequest<Response.Booru, Error.Booru>(new Response.Booru() {
                url = url,
                colorRating = color,
                saveId = saveId,
                tags = res.tags
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
            BooruSharp.Booru.Booru b = (BooruSharp.Booru.Booru)Activator.CreateInstance(elem.Item1, (BooruSharp.Booru.BooruAuth)null);
            List<string> artists = new List<string>();
            List<string> sources = new List<string>();
            List<string> characs = new List<string>();
            int i = 0;
            foreach (string s in elem.Item2.tags)
            {
                i++;
                try
                {
                    switch ((await b.GetTag(s)).type)
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
