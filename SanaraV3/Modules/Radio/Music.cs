namespace SanaraV3.Modules.Radio
{
    public sealed class Music
    {
        public Music(string path, string title, string url, string imageUrl, string requester)
        {
            Path = path;
            Title = title;
            Url = url;
            Downloading = false;
            ImageUrl = imageUrl;
            Requester = requester;
        }

        public string Path { private set; get; } // Local path to the file
        public string Title { private set; get; } // Title of the song
        public string Url { private set; get; } // Url to the YouTube video
        public bool Downloading { set; get; } // Is the file being downloaded
        public string ImageUrl { private set; get; } // Url of the image of the YouTube video
        public string Requester { private set; get; } // Discord name of the user who requested the song
    }
}
