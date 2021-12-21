namespace Sanara.Diaporama.Impl
{
    public record Doujinshi : IElement
    {
        public Doujinshi(string url, string imageUrl, string title, string[] tags, long id)
        {
            Url = url;
            ImageUrl = imageUrl;
            Title = title;
            Tags = tags;
            Id = id;
        }

        public string Url { get; }
        public string ImageUrl { get; }
        public string Title { get; }
        public string[] Tags { get; }
        public long Id { get; }
    }
}
