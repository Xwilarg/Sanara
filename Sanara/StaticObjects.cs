using BooruSharp.Booru;
using Discord;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Sentry;
using DiscordBotsList.Api;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Google.Cloud.Translation.V2;
using VndbSharp;
using System.Text.RegularExpressions;
using Sanara.Database;
using Sanara.Help;
using Sanara.Subscription;
using Sanara.Game;
using Sanara.Game.PostMode;
using Sanara.Game.Preload;
using Sanara.Game.Preload.Impl;
using Sanara.Module.Nsfw;

namespace Sanara
{
    /// <summary>
    /// Keep track of all the objects that must be keepen alive
    /// </summary>
    public static class StaticObjects
    {

       public static string BotName { get; } =
#if NSFW_BUILD
                "Sanara";
#else
                "Hanaki";
#endif
        public static DiscordSocketClient Client { get; } = new(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
        });
        /// <summary>
        /// To display uptime
        /// </summary>
        public static DateTime Started { set; get; }
        /// <summary>
        /// Guild used for debug
        /// Create guild slash command for there so it's faster to have access to new ones
        /// </summary>
        public static ulong DebugGuildId { set; get; }
        /// <summary>
        /// HttpClient used for all web requests
        /// </summary>
        public static HttpClient HttpClient { get; } = new();
        /// <summary>
        /// Random instance for number generation
        /// </summary>
        public static Random Random { get; } = new();

        /// <summary>
        /// Current ID of the bot
        /// </summary>
        public static ulong ClientId { set; get; }
        /// <summary>
        /// Access to the Database
        /// </summary>
        public static Db Db { get; } = new();
        /// <summary>
        /// Help object, contains help for all commands
        /// </summary>
        public static HelpPreload? Help { set; get; }
        /// <summary>
        /// List of all errors that occured
        /// </summary>
        public static Dictionary<string, System.Exception> Errors { get; } = new();
        /// <summary>
        /// Authentification token for discordbotlist.com
        /// </summary>
        private static string? DblToken { set; get; }
        /// <summary>
        /// Used to upload guild count to discordbotlist.com
        /// </summary>
        private static AuthDiscordBotListApi? DblApi { set; get; }
        /// <summary>
        /// Last request to discordbotlist.com
        /// </summary>
        private static DateTime DblLastSend { set; get; } = DateTime.Now;
        /// <summary>
        /// Last message received by the bot
        /// </summary>
        public static DateTime LastMessage { set; get; } = DateTime.UtcNow;

        // NSFW MODULE
        /// <summary>
        /// When downloading doujinshi, URL where the result is stored
        /// </summary>
        public static string? UploadWebsiteUrl { set; get; }
        /// <summary>
        /// When downloading doujinshi, location on the server where the doujinshi is stored
        /// </summary>
        public static string? UploadWebsiteLocation { set; get; }

        // BOORU INSTANCES
        public static Safebooru Safebooru { get; } = new();
        public static Gelbooru Gelbooru { get; } = new();
        public static E621 E621 { get; } = new();
        public static E926 E926 { get; } = new();
        public static Rule34 Rule34 { get; } = new();
        public static Konachan Konachan { get; } = new();
        public static Sakugabooru Sakugabooru { get; } = new(); // Only used in games
        /// <summary>
        /// List of tags when doing a booru search
        /// Used for commands asking more information about them
        /// </summary>
        public static TagsManager Tags { get; } = new TagsManager();
        /// <summary>
        /// Categories for Javmost
        /// </summary>
        public static List<string> JavmostCategories { get; } = new();

        // ENTERTAINMENT MODULE
        /// <summary>
        /// YouTube instance
        /// </summary>
        public static YouTubeService? YouTube { set; get; }
        /// <summary>
        /// Auth used for kitsu.io requests (Anime search)
        /// </summary>
        public static HttpRequestMessage? KitsuAuth { set; get; }
        /// <summary>
        /// Authentification token used for kitsu.io
        /// </summary>
        public static string? KitsuAccessToken { set; get; }
        public static DateTime KitsuRefreshDate { set; get; }
        public static string? KitsuRefreshToken { set; get; }
        /// <summary>
        /// VNDB client (visual novel search)
        /// </summary>
        public static Vndb VnClient { get; } = new();
        /// <summary>
        /// MyDramaList API key (drama search)
        /// </summary>
        public static string? MyDramaListApiKey { set; get; }
        /// <summary>
        /// Unsplash token (image search)
        /// </summary>
        public static string? UnsplashToken { set; get; }

        // GAME MODULE
        /// <summary>
        /// List of ongoing games
        /// </summary>
        public static List<AGame> Games { get; } = new();
        /// <summary>
        /// Upload mode where game just send a message in a textual channel
        /// </summary>
        public static TextMode ModeText { get; } = new();
        /// <summary>
        /// Upload mode where game send an URL containing an image
        /// </summary>
        public static UrlMode ModeUrl { get; } = new();
        /// <summary>
        /// Upload mode where game need to play an audio
        /// </summary>
        public static AudioMode ModeAudio { get; } = new();
        /// <summary>
        /// Preload data for games
        /// </summary>
        public static IPreload?[] Preloads { set; get; } = Array.Empty<IPreload>();
        /// <summary>
        /// List of all game names available
        /// </summary>
        public static string[] AllGameNames { set; get; } = Array.Empty<string>();
        /// <summary>
        /// Object managing games
        /// </summary>
        private static GameManager GM { get; } = new();
        public static Dictionary<string, BooruSharp.Search.Tag.TagType> QuizzTagsCache { get; } = new();
        public static Dictionary<string, BooruSharp.Search.Tag.TagType> GelbooruTags { get; } = new();

        // LANGUAGE MODULE
        // Dictionary managing convertion from/to hiragana/katakana/romaji
        public static Dictionary<string, string> RomajiToHiragana { set; get; } = new();
        public static Dictionary<string, string> HiraganaToRomaji { set; get; } = new();
        public static Dictionary<string, string> RomajiToKatakana { set; get; } = new();
        public static Dictionary<string, string> KatakanaToRomaji { set; get; } = new();

        /// <summary>
        /// Google Translate client
        /// </summary>
        public static TranslationClient? TranslationClient { set; get; }
        /// <summary>
        /// Image detector client, used to translate text directly from images
        /// </summary>
        public static ImageAnnotatorClient? VisionClient { set; get; }
        /// <summary>
        /// ISO 639-1 acronym to full language
        /// </summary>
        public static Dictionary<string, string> ISO639 { set; get; } = new()
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
        /// <summary>
        /// Discord flag emoticon to ISO 639-1
        /// </summary>
        public static Dictionary<string, string> Flags { set; get; } = new()
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
        /// <summary>
        /// Reverse the previous Dictionary
        /// </summary>
        public static Dictionary<string, string> ISO639Reverse { set; get; } = new();

        // DIAPORAMA
        /// <summary>
        /// List of diaporama
        /// Diaporama are embeds containing a list of elements where you can move using arrows emojis
        /// </summary>
        public static Dictionary<ulong, Diaporama.Diaporama> Diaporamas = new();

        // SUBSCRIPTION
        /// <summary>
        /// Subscription manager, follows a feed and post updates in a channel
        /// </summary>
        private static SubscriptionManager SM { get; } = new();

        public static Dictionary<string, int>? GetSubscriptionCount()
        {
            if (!SM.IsInit())
                return null;
            return SM.GetSubscriptionCount();
        }

        public static async Task<Dictionary<string, ITextChannel>?> GetSubscriptionsAsync(ulong guildId)
        {
            if (!SM.IsInit())
                return null;
            return await SM.GetSubscriptionsAsync(guildId);
        }

        private static async Task InitializeSubscriptions()
        {
            await SM.InitAsync();
            await Log.LogAsync(new LogMessage(LogSeverity.Info, "Static Preload", "Subscription initialized"));
        }

        private static async Task InitializeAV()
        {
            List<string> newTags;
            int page = 1;
            do
            {
                newTags = new(); // We keep track of how many tags we found in this page
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

            await Log.LogAsync(new LogMessage(LogSeverity.Info, "Static Preload", "AV initialized"));
        }

        public static async Task InitializeAsync(Credentials credentials)
        {
            await Log.LogAsync(new LogMessage(LogSeverity.Info, "Static Preload", "Initializing Static Objects"));
            await Db.InitAsync();

            if (credentials.DebugGuild != null)
            {
                DebugGuildId = ulong.Parse(credentials.DebugGuild);
            }

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", AppDomain.CurrentDomain.BaseDirectory + "/Keys/GoogleAPI.json");

            if (credentials.RavenKey != null)
            {
                SentrySdk.Init(credentials.RavenKey);
            }

            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Sanara");

            Safebooru.HttpClient = HttpClient;
            Gelbooru.HttpClient = HttpClient;
            E621.HttpClient = HttpClient;
            E926.HttpClient = HttpClient;
            Rule34.HttpClient = HttpClient;
            Konachan.HttpClient = HttpClient;

            RomajiToHiragana = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("LanguageResource/Hiragana.json"))!;
            foreach (var elem in RomajiToHiragana)
            {
                HiraganaToRomaji.Add(elem.Value, elem.Key);
            }
            RomajiToKatakana = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("LanguageResource/Katakana.json"))!;
            foreach (var elem in RomajiToKatakana)
            {
                KatakanaToRomaji.Add(elem.Value, elem.Key);
            }
            foreach (var elem in ISO639)
            {
                ISO639Reverse.Add(elem.Value, elem.Key);
            }

            await Log.LogAsync(new LogMessage(LogSeverity.Info, "Static Preload", "Loading game preload (might take several minutes if this is the first time)"));
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
            var preloads = new List<IPreload>();
            for (int i = 0; i < types.Length; i++)
            {
                var p = (IPreload)Activator.CreateInstance(types[i])!;
#if !NSFW_BUILD
                if (!p.IsSafe())
                {
                    await Log.LogAsync(new LogMessage(LogSeverity.Verbose, "Static Preload", types[i].ToString().Split('.').Last()[0..^7] + " was skipped"));
                    continue;
                }
#endif
                preloads.Add(p);
            }
            Preloads = preloads.ToArray();
            List<string> allNames = new();
            foreach (var p in Preloads)
            {
                if (p != null)
                {
                    var name = p.GetGameNames()[0];
                    var option = p.GetNameArg();
                    allNames.Add(name + (option == null ? "" : "-" + option));
                    _ = Task.Run( async() =>
                    {
                        try
                        {
                            p.Init();
                            await Log.LogAsync(new LogMessage(LogSeverity.Verbose, "Static Preload", p.GetType().ToString().Split('.').Last()[0..^7] + " successfully loaded"));
                        }
                        catch (System.Exception e)
                        {
                            await Log.LogErrorAsync(e, null);
                            await Log.LogAsync(new LogMessage(LogSeverity.Verbose, "Static Preload", p.GetType().ToString().Split('.').Last()[0..^7] + " failed to load"));
                        }
                    });
                }
            }
            AllGameNames = allNames.ToArray();

            _ = Task.Run(async () => { try { await InitializeSubscriptions(); } catch (System.Exception e) { await Log.LogErrorAsync(e, null); } });
            _ = Task.Run(async () => { try { await InitializeAV(); } catch (System.Exception e) { await Log.LogErrorAsync(e, null); } });

            await Log.LogAsync(new LogMessage(LogSeverity.Info, "Static Preload", "Initializing Game Manager"));
            GM.Init();

            await Log.LogAsync(new LogMessage(LogSeverity.Info, "Static Preload", "Initializing services needing credentials"));

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

            if (File.Exists("Keys/GoogleAPI.json"))
            {
                GoogleCredential googleCredentials = GoogleCredential.FromFile("Keys/GoogleAPI.json");
                TranslationClient = TranslationClient.Create();
                VisionClient = ImageAnnotatorClient.Create();
            }

            if (credentials.MyDramaListApiKey != null)
            {
                MyDramaListApiKey = credentials.MyDramaListApiKey;
            }

            UnsplashToken = credentials.UnsplashToken;

            await Log.LogAsync(new LogMessage(LogSeverity.Info, "Static Preload", "Static Preload done"));
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
