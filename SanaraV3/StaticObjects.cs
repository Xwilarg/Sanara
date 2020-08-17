using BooruSharp.Booru;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using SanaraV3.Diaporama;
using SanaraV3.Modules.Game;
using SanaraV3.Modules.Game.PostMode;
using SanaraV3.Modules.Game.Preload;
using SanaraV3.Modules.Game.Preload.Shiritori;
using SanaraV3.Modules.Nsfw;
using SanaraV3.Modules.Radio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace SanaraV3
{
    /// <summary>
    /// Keep track of all the objects that must be keepen alive
    /// </summary>
    public static class StaticObjects
    {
        public static readonly HttpClient HttpClient = new HttpClient();
        public static readonly Random Random = new Random();

        public static ulong ClientId;

        // NSFW MODULE
        public static readonly Safebooru Safebooru   = new Safebooru();
        public static readonly Gelbooru  Gelbooru    = new Gelbooru();
        public static readonly E621      E621        = new E621();
        public static readonly E926      E926        = new E926();
        public static readonly Rule34    Rule34      = new Rule34();
        public static readonly Konachan  Konachan    = new Konachan();
        public static readonly TagsManager Tags      = new TagsManager();

        // RADIO MODULE
        public static readonly Dictionary<ulong, RadioChannel> Radios = new Dictionary<ulong, RadioChannel>();

        // ENTERTAINMENT MODULE
        public static YouTubeService YouTube = null;

        // GAME MODULE
        public static readonly List<AGame>   Games = new List<AGame>();
        public static readonly TextMode      ModeText = new TextMode();
        public static readonly UrlMode ModeUrl = new UrlMode();
        public static readonly IPreload[]    Preloads;
        private static readonly GameManager _gm = new GameManager();

        // LANGUAGE MODULE
        public static readonly Dictionary<string, string> RomajiToHiragana = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> HiraganaToRomaji = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> RomajiToKatakana = new Dictionary<string, string>();
        public static readonly Dictionary<string, string> KatakanaToRomaji = new Dictionary<string, string>();

        // DIAPORAMA
        public static Dictionary<ulong, Diaporama.Diaporama> Diaporamas = new Dictionary<ulong, Diaporama.Diaporama>();

        static StaticObjects()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Sanara");

            Safebooru.HttpClient = HttpClient;
            Gelbooru.HttpClient = HttpClient;
            E621.HttpClient = HttpClient;
            E926.HttpClient = HttpClient;
            Rule34.HttpClient = HttpClient;
            Konachan.HttpClient = HttpClient;

            RomajiToHiragana = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("LanguageResources/Hiragana.json"));
            foreach (var elem in RomajiToHiragana)
            {
                HiraganaToRomaji.Add(elem.Value, elem.Key);
            }
            RomajiToKatakana = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("LanguageResources/Katakana.json"));
            foreach (var elem in RomajiToKatakana)
            {
                KatakanaToRomaji.Add(elem.Value, elem.Key);
            }

            Preloads = new[]
            {
                new ShiritoriPreload()
            };
        }

        public static void Initialize(Credentials credentials)
        {
            if (credentials.YouTubeKey != null)
            {
                YouTube = new YouTubeService(new BaseClientService.Initializer
                {
                    ApiKey = credentials.YouTubeKey
                });
            }
        }
    }
}
