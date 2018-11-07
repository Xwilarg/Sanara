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
            public float rating;
            public DateTime? startDate;
            public DateTime? endDate;
            public string ageRating;
            public string synopsis;
        }
    }
}
