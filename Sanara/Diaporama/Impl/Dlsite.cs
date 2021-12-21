namespace Sanara.Diaporama.Impl
{
    public record Dlsite : IElement
    {
        public Dlsite(string url, string imageUrl, string title, long id, float? rating, string nbDownload, string price, string description, string[] tags, string type)
        {
            Url = url;
            ImageUrl = imageUrl;
            Title = title;
            Id = id;
            Rating = rating;
            NbDownload = nbDownload;
            Price = price;
            Description = description;
            Tags = tags;
            Type = type;
        }

        public string Url { get; }
        public string ImageUrl { get; }
        public string Title { get; }
        public long Id { get; }
        public float? Rating { get; }
        public string NbDownload { get; }
        public string Price { get; }
        public string Description { get; }
        public string[] Tags { get; }
        public string Type { get; }
    }
}
