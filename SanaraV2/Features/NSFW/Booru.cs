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
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Features.NSFW
{
    public static class Booru
    {
        public static async Task<FeatureRequest<Response.Booru, Error.Booru>> SearchBooru(bool isChanSafe, string[] tags, BooruSharp.Booru.Booru booru)
        {
            if (!booru.IsSafe() && isChanSafe)
                return (new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.ChanNotNSFW));
            var res = await booru.GetRandomImage(tags);
            string url = res.fileUrl.AbsoluteUri;
            if (!Utilities.IsImage(url.Split('.').Last()))
                return (new FeatureRequest<Response.Booru, Error.Booru>(null, Error.Booru.InvalidFile));
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
            return (new FeatureRequest<Response.Booru, Error.Booru>(new Response.Booru() {
                    url = url,
                    colorRating = color
                }, Error.Booru.None));
        }
    }
}
