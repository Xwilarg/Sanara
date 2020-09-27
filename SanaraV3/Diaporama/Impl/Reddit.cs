using System;

namespace SanaraV3.Diaporama.Impl
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

        public string Title { get; }
        public Uri Image { get; }
        public Uri Link { get; }
        public int Ups { get; }
        public string Flairs { get; }
        public bool IsNsfw { get; }
        public string Content { get; }
    }
}
