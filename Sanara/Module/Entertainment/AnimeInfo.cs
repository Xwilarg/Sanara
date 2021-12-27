namespace Sanara.Module.Entertainment
{
    public record AnimeInfo
    {
        public Attributes Attributes;
    }

    public record Attributes
    {
        public string Subtype;
        public bool Nsfw;
        public string Synopsis;
        public string CanonicalTitle;
        public string Slug;
        public PosterImage PosterImage;
        public Titles Titles;
        public string AverageRating;
        public string EpisodeCount;
        public string EpisodeLength;
        public string StartDate;
        public string EndDate;
        public string AgeRatingGuide;
    }

    public record PosterImage
    {
        public string Original;
    }

    public record Titles
    {
        public string En;
        public string En_jp;
        public string En_us;
    }
}
