using System;

namespace SanaraV3.Diaporama
{
    public sealed class Reddit : IElement
    {
        public Reddit(string title, Uri image, Uri link, int ups, string flairs, bool isNsfw, string content)
        {
            Title = title;
            Image = image;
            Link = link;
            Ups = ups;
            Flairs = flairs;
            IsNsfw = isNsfw;
            Content = content;
        }

        public string Title;
        public Uri Image;
        public Uri Link;
        public int Ups;
        public string Flairs;
        public bool IsNsfw;
        public string Content;
    }
}
