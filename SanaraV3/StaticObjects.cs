﻿using BooruSharp.Booru;
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
using System.Linq;
using System.Text.RegularExpressions;

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
        public static string[] AllowedBots { set; get; } = new string[0];
        public static DateTime LastMessage { set; get; } = DateTime.UtcNow;

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
        public static List<string> JavmostCategories { get; } = new List<string>();

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
        public static string[] AllGameNames { set; get; }
        private static GameManager GM { get; } = new GameManager();
        public static Dictionary<string, BooruSharp.Search.Tag.TagType> QuizzTagsCache { get; } = new Dictionary<string, BooruSharp.Search.Tag.TagType>();
        public static Dictionary<string, BooruSharp.Search.Tag.TagType> GelbooruTags { get; } = new Dictionary<string, BooruSharp.Search.Tag.TagType>();

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
        private static SubscriptionManager SM { get; } = new SubscriptionManager();

        public static Dictionary<string, int> GetSubscriptionCount()
        {
            if (!SM.IsInit())
                return null;
            return SM.GetSubscriptionCount();
        }

        public static async Task<Dictionary<string, ITextChannel>> GetSubscriptionsAsync(ulong guildId)
        {
            if (!SM.IsInit())
                return null;
            return await SM.GetSubscriptionsAsync(guildId);
        }

        private static async Task InitializeSubscriptions()
        {
            await SM.InitAsync();
            await Utils.Log(new LogMessage(LogSeverity.Info, "Static Preload", "Subscription initialized"));
        }

        private static async Task InitializeAV()
        {
            List<string> newTags;
            int page = 1;
            do
            {
                newTags = new List<string>(); // We keep track of how many tags we found in this page
                string html = await VideoModule.DoJavmostHttpRequestAsync("https://www5.javmost.com/allcategory/" + page);
                foreach (Match m in Regex.Matches(html, "<a href=\"https:\\/\\/www5\\.javmost\\.com\\/category\\/([^\\/]+)\\/\">").Cast<Match>())
                {
                    string content = m.Groups[1].Value.Trim().ToLower();
                    if (!JavmostCategories.Contains(content)) // Make sure to not add things twice
                        newTags.Add(content);
                }
                JavmostCategories.AddRange(newTags);
                page++;
            } while (newTags.Count > 0);

            if (JavmostCategories.Count == 0) // This mean we weren't able to load the other tags
            {
                // There 2 tags aren't in the tag list so we add them manually
                JavmostCategories.Add("censor");
                JavmostCategories.Add("uncensor");
            }

            await Utils.Log(new LogMessage(LogSeverity.Info, "Static Preload", "AV initialized"));
        }

        public static async Task InitializeAsync(Credentials credentials)
        {
            await Utils.Log(new LogMessage(LogSeverity.Info, "Static Preload", "Initializing Static Objects"));
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
            Type[] types = new[]
            {
                // AUDIO
                typeof(ArknightsAudioPreload),
                typeof(KancolleAudioPreload),

                // HARD
                typeof(ShiritoriHardPreload),

                // OTHERS
                typeof(ShiritoriPreload),
                typeof(ArknightsPreload),
                typeof(KancollePreload),
                typeof(GirlsFrontlinePreload),
                typeof(AzurLanePreload),
                typeof(FateGOPreload),
                typeof(PokemonPreload),
                typeof(AnimePreload),
                typeof(BooruQuizzPreload),
                typeof(BooruFillPreload)
            };
            Preloads = new IPreload[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                try
                {
                    Preloads[i] = (IPreload)Activator.CreateInstance(types[i]);
                    await Utils.Log(new LogMessage(LogSeverity.Verbose, "Static Preload", types[i].ToString().Split('.').Last()[0..^7] + " successfully loaded"));
                }
                catch (System.Exception e)
                {
                    Preloads[i] = null;
                    await Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                    await Utils.Log(new LogMessage(LogSeverity.Verbose, "Static Preload", types[i].ToString().Split('.').Last()[0..^7] + " failed to load"));
                }
            }
            List<string> allNames = new List<string>();
            foreach (var p in Preloads)
            {
                var name = p.GetGameNames()[0];
                var option = p.GetNameArg();
                allNames.Add(name + (option == null ? "" : "-" + option));
            }
            AllGameNames = allNames.ToArray();

            _ = Task.Run(async () => { try { await InitializeSubscriptions(); } catch (System.Exception e) { await Utils.LogErrorAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e)); } });
            _ = Task.Run(async () => { try { await InitializeAV(); } catch (System.Exception e) { await Utils.LogErrorAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e)); } });

            await Utils.Log(new LogMessage(LogSeverity.Info, "Static Preload", "Initializing Game Manager"));
            GM.Init();

            await Utils.Log(new LogMessage(LogSeverity.Info, "Static Preload", "Initializing services needing credentials"));

            if (File.Exists("Saves/Premium.txt"))
                AllowedPremium = File.ReadAllLines("Saves/Premium.txt");

            AllowedBots = credentials.AllowedBots;

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

            await Utils.Log(new LogMessage(LogSeverity.Info, "Static Preload", "Static Preload done"));
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
