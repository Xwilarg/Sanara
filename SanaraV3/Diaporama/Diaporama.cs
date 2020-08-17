namespace SanaraV3.Diaporama
{
    public class Diaporama
    {
        public Diaporama(IElement[] elements)
        {
            CurrentPage = 0;
            Elements = elements;
        }

        public int CurrentPage;
        public IElement[] Elements;
    }
}
