using BooruSharp.Booru;
using Google.Apis.YouTube.v3;
using SanaraV3.Modules.Game;
using SanaraV3.Modules.Game.PostMode;
using SanaraV3.Modules.Nsfw;
using SanaraV3.Modules.Radio;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SanaraV3
{
    /// <summary>
    /// Keep track of all the objects that must be keepen alive
    /// </summary>
    public static class StaticObjects
    {
        public static HttpClient HttpClient = new HttpClient();
        public static Random Random = new Random();

        // NSFW MODULE
        public static Safebooru Safebooru   = new Safebooru();
        public static Gelbooru  Gelbooru    = new Gelbooru();
        public static E621      E621        = new E621();
        public static E926      E926        = new E926();
        public static Rule34    Rule34      = new Rule34();
        public static Konachan  Konachan    = new Konachan();
        public static TagsManager Tags      = new TagsManager();

        // RADIO MODULE
        public static Dictionary<ulong, RadioChannel> Radios = new Dictionary<ulong, RadioChannel>();

        // ENTERTAINMENT MODULE
        public static YouTubeService YouTube = null;

        // GAME MODULE
        public static List<AGame> Games = new List<AGame>();
        public static TextMode ModeText = new TextMode();
        public static UrlMode ModeUrl = new UrlMode();

        static StaticObjects()
        {
            Safebooru.HttpClient = HttpClient;
            Gelbooru.HttpClient = HttpClient;
            E621.HttpClient = HttpClient;
            E926.HttpClient = HttpClient;
            Rule34.HttpClient = HttpClient;
            Konachan.HttpClient = HttpClient;
        }
    }
}
