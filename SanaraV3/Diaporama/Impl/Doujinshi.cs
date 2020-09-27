namespace SanaraV3.Diaporama.Impl
{
    public class Doujinshi : IElement
    {
        public Doujinshi(string url, string imageUrl, string title, string[] tags, long id)
        {
            Url = url;
            ImageUrl = imageUrl;
            Title = title;
            Tags = tags;
            Id = id;
        }

        public string Url;
        public string ImageUrl;
        public string Title;
        public string[] Tags;
        public long Id;
    }
}
