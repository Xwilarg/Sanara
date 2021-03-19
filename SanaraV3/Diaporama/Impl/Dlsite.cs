namespace SanaraV3.Diaporama.Impl
{
    public class Dlsite : IElement
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

        public string Url;
        public string ImageUrl;
        public string Title;
        public long Id;
        public float? Rating;
        public string NbDownload;
        public string Price;
        public string Description;
        public string[] Tags;
        public string Type;
    }
}
