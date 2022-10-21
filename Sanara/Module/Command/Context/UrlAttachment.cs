using Discord;

namespace Sanara.Module.Command.Context
{
    public class UrlAttachment : IAttachment
    {
        public UrlAttachment(string url)
        {
            Url = url;
        }

        public ulong Id => throw new NotImplementedException();

        public string Filename => Path.GetFileName(new Uri(Url).LocalPath);

        public string Url { init; get; }

        public string ProxyUrl => throw new NotImplementedException();

        public int Size => throw new NotImplementedException();

        public int? Height => throw new NotImplementedException();

        public int? Width => throw new NotImplementedException();

        public bool Ephemeral => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public string ContentType => throw new NotImplementedException();
    }
}
