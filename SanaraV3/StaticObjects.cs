using BooruSharp.Booru;
using Discord;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using SanaraV3.Database;
using SanaraV3.Game;
using SanaraV3.Game.PostMode;
using SanaraV3.Game.Preload;
using SanaraV3.Game.Preload.Impl;
using SanaraV3.Module.Nsfw;
using SanaraV3.Module.Radio;
using SanaraV3.Subscription;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DiscordUtils;
using SharpRaven;
using SanaraV3.Help;
using DiscordBotsList.Api;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Google.Cloud.Translation.V2;
using SanaraV3.StatUpload;
using VndbSharp;

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
        public static RavenClient RavenClient { set; get; } = null;
        public static Dictionary<ulong, ErrorData> Errors { get; } = new Dictionary<ulong, ErrorData>(); // All errors that occured
        private static string DblToken { set; get; }
        private static AuthDiscordBotListApi DblApi { set; get; } = null;
        private static DateTime DblLastSend { set; get; } = DateTime.Now;
        public static UploadManager Website { set; get; } = null;

        // INFORMATION MODULE
        public static string GithubKey { set; get; }

        // NSFW MODULE
        public static string UploadWebsiteUrl { set; get; }
        public static string UploadWebsiteLocation { set; get; }

        public static Safebooru Safebooru { get; } = new Safebooru();
        public static Gelbooru Gelbooru { get; } = new Gelbooru();
        public static E621 E621 { get; } = new E621();
        public static E926 E926 { get; } = new E926();
        public static Rule34 Rule34 { get; } = new Rule34();
        public static Konachan Konachan { get; } = new Konachan();
        public static Sakugabooru Sakugabooru { get; } = new Sakugabooru();
        public static TagsManager Tags { get; } = new TagsManager();

        // RADIO MODULE
        public static Dictionary<ulong, RadioChannel> Radios { get; } = new Dictionary<ulong, RadioChannel>();
        public static string[] AllowedPremium { set; get; } = new string[0];

        // ENTERTAINMENT MODULE
        public static YouTubeService YouTube { set; get; }
        public static HttpRequestMessage KitsuAuth { set; get; } = null;
        public static string KitsuAccessToken { set; get; } = null;
        public static DateTime KitsuRefreshDate { set; get; }
        public static string KitsuRefreshToken { set; get; }
        public static Vndb VnClient { get; } = new Vndb();
        public static string MyDramaListApiKey { set; get; }
        public static string UnsplashToken { set; get; }

        // GAME MODULE
        public static List<AGame> Games { get; } = new List<AGame>();
        public static TextMode ModeText { get; } = new TextMode();
        public static UrlMode ModeUrl { get; } = new UrlMode();
        public static AudioMode ModeAudio { get; } = new AudioMode();
        public static IPreload[] Preloads { set; get; }
        private static GameManager GM { get; } = new GameManager();
        public static Dictionary<string, BooruSharp.Search.Tag.TagType> QuizzTagsCache { get; } = new Dictionary<string, BooruSharp.Search.Tag.TagType>();

        // LANGUAGE MODULE
        public static Dictionary<string, string> RomajiToHiragana { set; get; } = new Dictionary<string, string>();
        public static Dictionary<string, string> HiraganaToRomaji { set; get; } = new Dictionary<string, string>();
        public static Dictionary<string, string> RomajiToKatakana { set; get; } = new Dictionary<string, string>();
        public static Dictionary<string, string> KatakanaToRomaji { set; get; } = new Dictionary<string, string>();

        public static TranslationClient TranslationClient { set; get; } = null;
        public static ImageAnnotatorClient VisionClient { set; get; } = null;
        public static Dictionary<string, string> ISO639 { set; get; } = new Dictionary<string, string>
        {
            { "fr", "french" },
            { "en", "english" },
            { "ja", "japanese" },
            { "ru", "russian" },
            { "zh", "chinese" },
            { "ko", "korean" },
            { "ge", "german" },
            { "es", "spanish" },
            { "nl", "dutch" }
        };
        public static Dictionary<string, string> Flags { set; get; } = new Dictionary<string, string>
        {
            { "🇫🇷", "fr" },
            { "🇺🇸", "en" },
            { "🇬🇧", "en" },
            { "🇯🇵", "ja" },
            { "🇷🇺", "ru" },
            { "🇹🇼", "zh" },
            { "🇨🇳", "zh" },
            { "🇰🇷", "ko" },
            { "🇩🇪", "ge" },
            { "🇪🇸", "es" },
            { "🇳🇱", "nl" }
        };
        public static Dictionary<string, string> ISO639Reverse { set; get; } = new Dictionary<string, string>();

        // DIAPORAMA
        public static Dictionary<ulong, Diaporama.Diaporama> Diaporamas = new Dictionary<ulong, Diaporama.Diaporama>();

        // SUBSCRIPTION
        public static SubscriptionManager SM { get; } = new SubscriptionManager();

        public static async Task InitializeAsync(Credentials credentials)
        {
            await Db.InitAsync("Sanara");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", AppDomain.CurrentDomain.BaseDirectory + "/Keys/GoogleAPI.json");

            if (credentials.RavenKey != null)
            {
                RavenClient = new RavenClient(credentials.RavenKey);
            }

            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Sanara");

            Safebooru.HttpClient = HttpClient;
            Gelbooru.HttpClient = HttpClient;
            E621.HttpClient = HttpClient;
            E926.HttpClient = HttpClient;
            Rule34.HttpClient = HttpClient;
            Konachan.HttpClient = HttpClient;

            RomajiToHiragana = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("LanguageResource/Hiragana.json"));
            foreach (var elem in RomajiToHiragana)
            {
                HiraganaToRomaji.Add(elem.Value, elem.Key);
            }
            RomajiToKatakana = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("LanguageResource/Katakana.json"));
            foreach (var elem in RomajiToKatakana)
            {
                KatakanaToRomaji.Add(elem.Value, elem.Key);
            }
            foreach (var elem in ISO639)
            {
                ISO639Reverse.Add(elem.Value, elem.Key);
            }

            await Utils.Log(new LogMessage(LogSeverity.Info, "Static Preload", "Loading game preload (might take several minutes if this is the first time)"));
            Preloads = new IPreload[]
            {
                new ShiritoriPreload(),
                new ArknightsPreload(),
                new ArknightsAudioPreload(),
                new KancollePreload(),
                new KancolleAudioPreload(),
                new GirlsFrontlinePreload(),
                new AzurLanePreload(),
                new FateGOPreload(),
                new PokemonPreload(),
                new AnimePreload(),
                new BooruQuizzPreload(),
                new BooruFillPreload()
            };
            await Utils.Log(new LogMessage(LogSeverity.Info, "Static Preload", "Game preload done"));

            await SM.InitAsync();
            GM.Init();

            if (File.Exists("Saves/Premium.txt"))
                AllowedPremium = File.ReadAllLines("Saves/Premium.txt");

            if (credentials.YouTubeKey != null)
            {
                YouTube = new YouTubeService(new BaseClientService.Initializer
                {
                    ApiKey = credentials.YouTubeKey
                });
            }

            if (credentials.KitsuEmail != null)
            {
                KitsuAuth = new HttpRequestMessage(HttpMethod.Post, "https://kitsu.io/api/oauth/token")
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "grant_type", "password" },
                        { "username", credentials.KitsuEmail },
                        { "password", credentials.KitsuPassword }
                    })
                };
            }

            if (credentials.UploadWebsiteLocation != null)
            {
                UploadWebsiteLocation = credentials.UploadWebsiteLocation;
                if (!UploadWebsiteLocation.EndsWith("/")) UploadWebsiteLocation += "/";

                UploadWebsiteUrl = credentials.UploadWebsiteUrl;
                if (!UploadWebsiteUrl.EndsWith("/")) UploadWebsiteUrl += "/"; // Makes sure the URL end with a /
            }

            if (credentials.TopGgToken != null)
            {
                DblToken = credentials.TopGgToken;
            }

            if (credentials.GithubKey != null)
            {
                GithubKey = credentials.GithubKey;
            }

            if (File.Exists("Keys/GoogleAPI.json"))
            {
                GoogleCredential googleCredentials = GoogleCredential.FromFile("Keys/GoogleAPI.json");
                TranslationClient = TranslationClient.Create();
                VisionClient = ImageAnnotatorClient.Create();
            }

            if (credentials.StatsWebsiteUrl != null)
            {
                Website = new UploadManager(credentials.StatsWebsiteUrl, credentials.StatsWebsiteToken);
            }

            if (credentials.MyDramaListApiKey != null)
            {
                MyDramaListApiKey = credentials.MyDramaListApiKey;
            }

            UnsplashToken = credentials.UnsplashToken;
        }

        public static async Task UpdateTopGgAsync()
        {
            if (DblToken != null)
            {
                if (DblApi == null)
                    DblApi = new AuthDiscordBotListApi(ClientId, DblToken);
                if (DblLastSend.AddMinutes(10).CompareTo(DateTime.Now) < 0) // Make sure to not spam the API
                {
                    DblLastSend = DateTime.Now;
                    await DblApi.UpdateStats(Client.Guilds.Count);
                }
            }
        }
    }
}
