using Discord;

namespace SanaraV3.Modules.Radio
{
    public sealed class Music
    {
        public Music(string path, string title, string url, Embed embed, string requester)
        {
            Path = path;
            Title = title;
            Url = url;
            Downloading = false;
            Embed = embed;
            Requester = requester;
        }

        public string Path { private set; get; } // Local path to the file
        public string Title { private set; get; } // Title of the song
        public string Url { private set; get; } // Url to the YouTube video
        public bool Downloading { set; get; } // Is the file being downloaded
        public Embed Embed { private set; get; } // Embed to be sent when the song begin
        public string Requester { private set; get; } // User that requested the music
    }
}
