using BooruSharp.Booru;
using Discord;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using SanaraV3.Database;
using SanaraV3.Modules.Administration;
using SanaraV3.Modules.Game;
using SanaraV3.Modules.Game.PostMode;
using SanaraV3.Modules.Game.Preload;
using SanaraV3.Modules.Game.Preload.Impl;
using SanaraV3.Modules.Nsfw;
using SanaraV3.Modules.Radio;
using SanaraV3.Subscription;
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
        public static DiscordSocketClient Client { get; } = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
        });
        public static HttpClient HttpClient { get; } = new HttpClient();
        public static Random Random { get; } = new Random();

        public static ulong ClientId;
        public static Db Db { get; } = new Db();
        public static HelpPreload Help { get; } = new HelpPreload();

        // NSFW MODULE
        public static string UploadWebsiteUrl { set; get; }
        public static string UploadWebsiteLocation { set;  get; }

        public static Safebooru Safebooru    { get; } = new Safebooru();
        public static Gelbooru  Gelbooru     { get; } = new Gelbooru();
        public static E621      E621         { get; } = new E621();
        public static E926      E926         { get; } = new E926();
        public static Rule34    Rule34       { get; } = new Rule34();
        public static Konachan  Konachan     { get; } = new Konachan();
        public static TagsManager Tags       { get; } = new TagsManager();

        // RADIO MODULE
        public static Dictionary<ulong, RadioChannel> Radios { get; } = new Dictionary<ulong, RadioChannel>();

        // ENTERTAINMENT MODULE
        public static YouTubeService YouTube { set;  get; }

        // GAME MODULE
        public static List<AGame> Games { get; } = new List<AGame>();
        public static TextMode ModeText { get; } = new TextMode();
        public static UrlMode ModeUrl { get; } = new UrlMode();
        public static AudioMode ModeAudio { get; } = new AudioMode();
        public static IPreload[] Preloads { get; }
        private static GameManager GM { get; } = new GameManager();

        // LANGUAGE MODULE
        public static Dictionary<string, string> RomajiToHiragana { get; } = new Dictionary<string, string>();
        public static Dictionary<string, string> HiraganaToRomaji { get; } = new Dictionary<string, string>();
        public static Dictionary<string, string> RomajiToKatakana { get; } = new Dictionary<string, string>();
        public static Dictionary<string, string> KatakanaToRomaji { get; } = new Dictionary<string, string>();

        // DIAPORAMA
        public static Dictionary<ulong, Diaporama.Diaporama> Diaporamas = new Dictionary<ulong, Diaporama.Diaporama>();

        // SUBSCRIPTION
        public static SubscriptionManager SM { get; } = new SubscriptionManager();
        public static List<SubscriptionGuild> NHentaiSubscriptions { get; } = new List<SubscriptionGuild>();
        public static List<SubscriptionGuild> AnimeSubscriptions { get; } = new List<SubscriptionGuild>(;)

        static StaticObjects()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Sanara");

            Db.InitAsync("Sanara").GetAwaiter().GetResult();

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

            Preloads = new IPreload[]
            {
                new ShiritoriPreload(),
                new ArknightsPreload(),
                new ArknightsAudioPreload(),
                new KancollePreload(),
                new KancolleAudioPreload()
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

            if (credentials.UploadWebsiteLocation != null)
            {
                UploadWebsiteLocation = credentials.UploadWebsiteLocation;
                if (!UploadWebsiteLocation.EndsWith("/")) UploadWebsiteLocation += "/";

                UploadWebsiteUrl = credentials.UploadWebsiteUrl;
                if (!UploadWebsiteUrl.EndsWith("/")) UploadWebsiteUrl += "/"; // Makes sure the URL end with a /
            }
        }
    }
}
