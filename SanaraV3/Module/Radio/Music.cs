using Discord;
using System;

namespace SanaraV3.Module.Radio
{
    public sealed class Music
    {
        public Music(string id, string path, string title, Uri url, Embed embed, string requester, bool isAutoSuggestion, string duration)
        {
            Id = id;
            Path = path;
            Title = title;
            Url = url;
            Downloading = false;
            Embed = embed;
            Requester = requester;
            IsAutoSuggestion = isAutoSuggestion;
            Duration = duration;
        }

        public string Id { private set; get; } // ID of the video
        public string Path { private set; get; } // Local path to the file
        public string Title { private set; get; } // Title of the song
        public Uri Url { private set; get; } // Url to the YouTube video
        public bool Downloading { set; get; } // Is the file being downloaded
        public Embed Embed { private set; get; } // Embed to be sent when the song begin
        public string Requester { private set; get; } // User that requested the music
        public bool IsAutoSuggestion { private set; get; } // Is the music an autosuggestion
        public string Duration { private set; get; } // Duration of the song
    }
}
