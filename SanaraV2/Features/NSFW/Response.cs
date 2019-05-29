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

namespace SanaraV2.Features.NSFW
{
    public static class Response
    {
        public class Booru
        {
            public string url;
            public Color colorRating;
            public string saveId;
            public string[] tags;
        }

        public class BooruTags
        {
            public string[] sourceTags;
            public string[] artistTags;
            public string[] characTags;
            public Uri imageUrl;
            public Color rating;
            public string booruName;
            public int height;
            public int width;
            public Tuple<long, long> aspectRatio;
        }

        public class Doujinshi
        {
            public string url;
            public string title;
            public string[] tags;
            public string imageUrl;
        }
    }
}
