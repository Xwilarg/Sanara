namespace Sanara.Module.Utility
{
    public record AnimeInfo
    {
        public AnimeData data;
    }

    public record AnimeData
    {
        public AnimeContainer anime;
    }

    public record AnimeContainer
    {
        public AnimeResult[] media;
    }

    public record AnimeResult
    {
        public int id;
        public AnimeTitle title;
        public bool isAdult;
        public string description;
        public AnimeCover coverImage;
        public int? averageScore;
        public int? episodes;
        public int? duration;
        public FuzzyDate startDate;
        public FuzzyDate endDate;
        public string source;
        public string format;
        public string type;
    }

    public record AnimeTitle
    {
        public string romaji;
        public string native;
        public string english;
    }

    public record AnimeCover
    {
        public string large;
    }

    public record FuzzyDate
    {
        public int? year;
        public int? month;
        public int? day;
    }
}
