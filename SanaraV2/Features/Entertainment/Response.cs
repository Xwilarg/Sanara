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

namespace SanaraV2.Features.Entertainment
{
    public static class Response
    {
        public class AnimeManga
        {
            public string name;
            public string imageUrl;
            public string[] alternativeTitles;
            public int? episodeCount;
            public int? episodeLength;
            public float? rating;
            public DateTime? startDate;
            public DateTime? endDate;
            public string ageRating;
            public string synopsis;
        }

        public class Xkcd
        {
            public string imageUrl;
            public int maxNb;
            public string title;
            public string alt;
        }

        public class YouTube
        {
            public string url;
            public string name;
            public string imageUrl;
        }

        public class Game
        {
            public bool isNormal;
            public GameName gameName;
        }

        public enum GameName
        {
            Booru,
            Kancolle,
            Anime,
            Shiritori,
            FireEmblem
        }
    }
}
