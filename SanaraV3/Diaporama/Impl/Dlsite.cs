namespace SanaraV3.Diaporama.Impl
{
    public class Dlsite : IElement
    {
        public Dlsite(string url, string imageUrl, string title, long id, float? rating, string nbDownload, string price)
        {
            Url = url;
            ImageUrl = imageUrl;
            Title = title;
            Id = id;
            Rating = rating;
            NbDownload = nbDownload;
            Price = price;
        }

        public string Url;
        public string ImageUrl;
        public string Title;
        public long Id;
        public float? Rating;
        public string NbDownload;
        public string Price;
    }
}
