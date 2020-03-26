/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotsList.Api;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Cloud.Translation.V2;
using Google.Cloud.Vision.V1;
using Newtonsoft.Json;
using SanaraV2.Games;
using SanaraV2.Modules.Base;
using SanaraV2.Modules.Entertainment;
using SanaraV2.Modules.GamesInfo;
using SanaraV2.Modules.NSFW;
using SanaraV2.Modules.Tools;
using SanaraV2.Subscription;
using SharpRaven;
using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SanaraV2.Modules.Entertainment.RadioModule;

namespace SanaraV2
{
    public class Program
    {
        // If the bot didn't start after like 5 minutes we restart it (because that probably means it got stuck)
        private static bool isTimerValid;

        public static async Task Main()
        {
            try
            {
                await new Program().MainAsync();
            }
            catch (FileNotFoundException) // This probably means a dll is missing
            {
                throw;
            }
            catch (Exception) // If an exception occur, the program exit and is relaunched
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        public readonly DiscordSocketClient client;
        private readonly CommandService commands = new CommandService();
        public static Program p; // Allow to access the following variables from any module (contains an instance of Program)
        public Random rand; // Random numbers

        public GameManager gm; // See Games.GameManager

        // Translate command
        private GoogleCredential credential;
        public TranslationClient translationClient;
        public ImageAnnotatorClient visionClient;

        // YouTube and Radio modules
        public YouTubeService youtubeService;

        // Logs command
        public string GitHubKey;

        // Send stats to website
        private string websiteStats, websiteStatsToken;
        public bool sendStats { private set; get; }

        public string websiteUpload, websiteUploadToken; // For "Source" command

        // Discord Bot List
        private AuthDiscordBotListApi dblApi;
        private DateTime lastDiscordBotsSent; // We make sure to wait at least 10 mins before sending stats to DiscordBots.org so we don't spam the API

        // Information about guilds having launch radio (text channel where radio was launched and vocal channel where the bot is)
        public List<RadioChannel> radios;

        // When the bot was started
        public DateTime startTime;

        // For translation submodule
        public Dictionary<string, List<Translation.TranslationData>> translations;
        public Dictionary<string, List<string>> allLanguages;

        // Sentry (error reporting)
        private RavenClient ravenClient;

        public Db.Db db;

        private ulong inamiToken; // For unit tests

        // Anime/Manga module
        public Dictionary<string, string> kitsuAuth;

        // Subscriptions
        private SubscriptionManager subManager;

        // AV
        public List<string> categories;

        public Program()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Log;
            commands.Log += LogError;
        }

        public async Task MainAsync()
            => await MainAsync(null, 0);

        public async Task MainAsync(string botToken, ulong inamiId) // botToken is used for unit tests
        {
            isTimerValid = true;
            _ = Task.Run(async () =>
            {
                await Task.Delay(300000);
                if (isTimerValid)
                    Environment.Exit(1);
            });

            await Log(new LogMessage(LogSeverity.Info, "Setup", "Preparing bot"));
            inamiToken = inamiId;

            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            db = new Db.Db();
            await db.InitAsync();

            gm = new GameManager();

            p = this;

            rand = new Random();

            UpdateLanguageFiles();

            // Setting up various credentials
            if (botToken == null)
            {
                if (!File.Exists("Keys/Credentials.json"))
                    throw new FileNotFoundException("You must have a Credentials.json file located in " + AppDomain.CurrentDomain.BaseDirectory + "Keys, more information at https://sanara.zirk.eu/documentation.html?display=Clone");
                dynamic json = JsonConvert.DeserializeObject(File.ReadAllText("Keys/Credentials.json"));
                if (json.botToken == null || json.ownerId == null || json.ownerStr == null)
                    throw new NullReferenceException("Your Credentials.json is missing mandatory information, it must at least contains botToken, ownerId and ownerStr. More information at https://sanara.zirk.eu/documentation.html?display=Clone");
                botToken = json.botToken;
                Modules.Base.Sentences.ownerId = ulong.Parse((string)json.ownerId);
                Modules.Base.Sentences.ownerStr = json.ownerStr;

                GitHubKey = json.githubKey;
                websiteStats = json.websiteStats;
                websiteStatsToken = json.websiteStatsToken;
                websiteUpload = json.websiteUpload;
                websiteUploadToken = json.websiteUploadToken;
                if (json.kitsuEmail != null && json.kitsuPassword != null)
                {
                    kitsuAuth = new Dictionary<string, string> {
                    { "grant_type", "password" },
                    { "username", (string)json.kitsuEmail },
                    { "password", (string)json.kitsuPassword }
                };
                }
                else
                    kitsuAuth = null;
                sendStats = websiteStats != null && websiteStatsToken != null;
                if (json.discordBotsId != null && json.discordBotsToken != null)
                    dblApi = new AuthDiscordBotListApi(ulong.Parse((string)json.discordBotsId), (string)json.discordBotsToken);
                else
                    dblApi = null;
                await InitServices(json);
            }
            lastDiscordBotsSent = DateTime.MinValue;

            await commands.AddModuleAsync<Information>(null);
            await commands.AddModuleAsync<Settings>(null);
            await commands.AddModuleAsync<Linguist>(null);
            await commands.AddModuleAsync<Kancolle>(null);
            await commands.AddModuleAsync<Booru>(null);
            await commands.AddModuleAsync<VnModule>(null);
            await commands.AddModuleAsync<Doujinshi>(null);
            await commands.AddModuleAsync<AnimeManga>(null);
            await commands.AddModuleAsync<GameModule>(null);
            await commands.AddModuleAsync<Youtube>(null);
            await commands.AddModuleAsync<RadioModule>(null);
            await commands.AddModuleAsync<Xkcd>(null);
            await commands.AddModuleAsync<Communication>(null);
            await commands.AddModuleAsync<Code>(null);

            client.MessageReceived += HandleCommandAsync;
            client.GuildAvailable += GuildJoin;
            client.JoinedGuild += GuildJoin;
            client.JoinedGuild += GuildCountChange;
            client.LeftGuild += GuildCountChange;
            client.Disconnected += Disconnected;
            client.UserVoiceStateUpdated += VoiceUpdate;
            client.Connected += Connected;
            client.Ready += Ready;

            await client.LoginAsync(TokenType.Bot, botToken);
            startTime = DateTime.Now;
            await client.StartAsync();

            // Send stats every minute
            if (sendStats)
            {
                _ = Task.Run(async () =>
                {
                    for (;;)
                    {
                        await Task.Delay(60000); // 1 minute
                        if (client.ConnectionState == ConnectionState.Connected)
                            UpdateStatus();
                    }
                });
            }
            if (botToken == null) // Unit test manage the bot life
                await Task.Delay(-1);
        }

        /// <summary>
        /// When client is ready, we start the SubscriptionManager (we wait for it to be ready so it can access the guilds and channels)
        /// </summary>
        private Task Ready()
        {
            subManager = new SubscriptionManager();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Once the bot is connected we set his status and update his amount of server on DBL
        /// </summary>
        private async Task Connected()
        {
            isTimerValid = false;
            await client.SetGameAsync("https://sanara.zirk.eu", null, ActivityType.Watching);
            await UpdateDiscordBots();
        }

        /// <summary>
        /// Update amount of guilds on DBL when the bot join/leave a guild
        /// </summary>
        private async Task GuildCountChange(SocketGuild _)
        {
            await UpdateDiscordBots();
        }

        private async Task UpdateDiscordBots()
        {
            if (dblApi != null && lastDiscordBotsSent.AddMinutes(10).CompareTo(DateTime.Now) < 0)
            {
                lastDiscordBotsSent = DateTime.Now;
                await dblApi.UpdateStats(client.Guilds.Count);
            }
        }

        // We remove the bot from empty voice channels
        private async Task VoiceUpdate(SocketUser user, SocketVoiceState state, SocketVoiceState _)
        {
            RadioChannel radio = radios.Find(x => x.m_guildId == ((IGuildUser)user).GuildId);
            if (radio != null && await radio.IsChanEmpty())
            {
                await radio.Stop();
                radios.Remove(radio);
            }
        }

        // If the bot is disconnected we exit the program, an external program will relaunch it
        private Task Disconnected(Exception e)
        {
            if (!File.Exists("Saves/Logs"))
                Directory.CreateDirectory("Saves/Logs");
            File.WriteAllText("Saves/Logs/" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log", "Bot disconnected. Exception:\n" + e.ToString());
            return Task.CompletedTask;
        }

        private async Task InitServices(dynamic json)
        {
            // Update youtube-dl
            if (File.Exists("youtube-dl.exe"))
            {
                await Log(new LogMessage(LogSeverity.Info, "Setup", "Checking for youtube-dl updates"));
                ProcessStartInfo startInfo = new ProcessStartInfo("youtube-dl.exe", "-U");
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                Process process = Process.Start(startInfo);
                await Log(new LogMessage(LogSeverity.Info, "Setup", await process.StandardOutput.ReadToEndAsync()));
            }

            await Log(new LogMessage(LogSeverity.Info, "Setup", "Preloading games dictionnaries"));
            foreach (var elem in Constants.allGames)
            { }

            await Log(new LogMessage(LogSeverity.Info, "Setup", "Initialising services"));
            // Categories for AdultVideo command
            categories = new List<string>();
            categories.Add("censor");
            categories.Add("uncensor");
            List<string> newTags;
            int page = 1;
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    hc.Timeout = TimeSpan.FromSeconds(5.0); // There is probably smth like Cloudflare blocking the website
                    do
                    {
                        newTags = new List<string>();
                        string html = await hc.GetStringAsync("https://www5.javmost.com/allcategory/" + page);
                        foreach (Match m in Regex.Matches(html, "<a href=\"https:\\/\\/www5\\.javmost\\.com\\/category\\/([^\\/]+)\\/\">").Cast<Match>())
                        {
                            string content = m.Groups[1].Value.Trim().ToLower();
                            if (!categories.Contains(content))
                                newTags.Add(content);
                        }
                        categories.AddRange(newTags);
                        page++;
                    } while (newTags.Count > 0);
                }
            }
            catch (HttpRequestException)
            {
                // javmost isn't available in Korea so if we are in the debug version of the bot we ignore this
                if (!Debugger.IsAttached)
                    throw;
            }
            catch (TaskCanceledException)
            { } // Not available

            // Then we update all others modules
            // It's basically just checking if the credential file is here

            translationClient = null;
            if (json.googleTranslateJson != null)
            {
                try
                {
                    credential = GoogleCredential.FromFile((string)json.googleTranslateJson);
                    translationClient = TranslationClient.Create(credential);
                } catch (Exception e) {
                    await LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                }
            }

            youtubeService = null;
            if (json.youtubeKey != null)
            {
                try {
                    youtubeService = new YouTubeService(new BaseClientService.Initializer()
                    {
                        ApiKey = json.youtubeKey
                    });
                } catch (Exception e) {
                    await LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                }
            }

            radios = new List<RadioChannel>();

            ravenClient = null;
            if (json.ravenKey != null)
            {
                try {
                    ravenClient = new RavenClient((string)json.ravenKey);
                } catch (Exception e) {
                    await LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                }
            }

            visionClient = null;
            if (json.googleVisionJson != null)
            {
                try {
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", (string)json.googleVisionJson);
                    visionClient = ImageAnnotatorClient.Create();
                } catch (Exception e) {
                    await LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                }
            }
        }

        /// <summary>
        /// Delete the content of a folder recursively
        /// </summary>
        private void DeleteDirContent(string path)
        {
            foreach (string f in Directory.GetFiles(path))
                File.Delete(f);
            foreach (string d in Directory.GetDirectories(path))
            {
                DeleteDirContent(d);
                Directory.Delete(d);
            }
        }

        /// <summary>
        /// Copy translations from translations submodule to next to the executable
        /// </summary>
        private void CopyLanguagesFiles()
        {
            if (!Directory.Exists("Saves/Translations"))
            {
                Directory.CreateDirectory("Saves/Translations");
            }
            else if (Directory.Exists("../../Sanara-translations") && Directory.Exists("../../Sanara-translations/Translations"))
            {
                DeleteDirContent("Saves/Translations");
                foreach (string d in Directory.GetDirectories("../../Sanara-translations/Translations"))
                {
                    DirectoryInfo di = new DirectoryInfo(d);
                    File.Copy(d + "/terms.json", "Saves/Translations/terms-" + di.Name + ".json");
                    File.Copy(d + "/infos.json", "Saves/Translations/infos-" + di.Name + ".json");
                }
            }
        }

        /// <summary>
        /// Parse language files and update dictionnaries
        /// </summary>
        public void UpdateLanguageFiles()
        {
            allLanguages = new Dictionary<string, List<string>>();
            CopyLanguagesFiles();
            translations = new Dictionary<string, List<Translation.TranslationData>>();
            foreach (string f in Directory.GetFiles("Saves/Translations"))
            {
                FileInfo fi = new FileInfo(f);
                string lang = fi.Name.Split('.')[0].Split('-')[1];
                string[] content = File.ReadAllLines(f);
                foreach (string s in content)
                {
                    string part1 = "";
                    string part2 = "";
                    int partId = 0;
                    foreach (char c in s)
                    {
                        if (c == '"')
                            partId++;
                        else
                        {
                            if (partId == 1)
                                part1 += c;
                            else if (partId == 3)
                                part2 += c;
                        }
                    }
                    if (part1 != "" && part2 != "")
                    {
                        if (fi.Name.StartsWith("terms"))
                        {
                            if (!translations.ContainsKey(part1))
                                translations.Add(part1, new List<Translation.TranslationData>());
                            List<Translation.TranslationData> data = translations[part1];
                            if (!data.Any(x => x.language == lang))
                                data.Add(new Translation.TranslationData(lang, part2));
                        }
                        else
                        {
                            if (!allLanguages.ContainsKey(lang))
                                allLanguages.Add(lang, new List<string>());
                            allLanguages[lang].Add(part2);
                        }
                    }
                }
            }
            Log(new LogMessage(LogSeverity.Info, "Setup", "Translations updated")).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Init guilds in the db
        /// </summary>
        private async Task GuildJoin(SocketGuild arg)
        {
            await db.InitGuild(arg.Id);
        }

        public enum Module
        {
            AnimeManga,
            Booru,
            Code,
            Communication,
            Doujinshi,
            Game,
            Information,
            Kancolle,
            Linguistic,
            Radio,
            Settings,
            Vn,
            Xkcd,
            Youtube
        }

        /// <summary>
        /// Each commands call this functions that record datas about them
        /// </summary>
        /// <param name="u">User that sent the message</param>
        /// <param name="serverId">The ID of the current guild</param>
        /// <param name="m">The module that was called (see above)</param>
        public async Task DoAction(IUser u, ulong serverId, Module m)
        {
            if (!u.IsBot && sendStats)
                await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("modules", m.ToString()) });
        }

        /// <summary>
        /// Send stats to the website
        /// </summary>
        /// <param name="elems">Each tuple contains the name of the thing sent along with it value</param>
        public async Task UpdateElement(Tuple<string, string>[] elems)
        {
            HttpClient httpClient = new HttpClient();
            var values = new Dictionary<string, string> {
                           { "token", websiteStatsToken },
                           { "action", "add" },
                           { "name", "Sanara" }
                        };
            foreach (var elem in elems)
            {
                values.Add(elem.Item1, elem.Item2);
            }
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, websiteStats);
            msg.Content = new FormUrlEncodedContent(values);

            try
            {
                await httpClient.SendAsync(msg);
            }
            catch (HttpRequestException) // TODO: We should probably retry
            { }
            catch (TaskCanceledException)
            { }
        }

        /// Clean name before sending it to the website for stats (| and $ are delimitators so we remove them)
        public string GetName(string name)
            => name.Replace("|", "").Replace("$", "");

        private async void UpdateStatus()
        {
            // Server count
            List<Tuple<string, int>> guilds = new List<Tuple<string, int>>();
            foreach (IGuild g in client.Guilds)
            {
                guilds.Add(new Tuple<string, int>(g.Name, ((SocketGuild)g).MemberCount));
            }

            // Server biggest
            Tuple<string, int> biggest = null;
            string finalStr = "";
            for (int i = 0; i < 10; i++)
            {
                foreach (var tuple in guilds)
                {
                    if (biggest == null || tuple.Item2 > biggest.Item2)
                        biggest = tuple;
                }
                if (biggest == null)
                    break;
                finalStr += GetName(biggest.Item1) + "|" + biggest.Item2 + "$";
                guilds.Remove(biggest);
                biggest = null;
            }

            await UpdateElement(new Tuple<string, string>[] {   new Tuple<string, string>("serverCount", client.Guilds.Count.ToString()),
                                                                new Tuple<string, string>("serversBiggest", finalStr),
                                                                new Tuple<string, string>("bestScores", await ScoreManager.GetBestScores())});
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg.Author.Id == client.CurrentUser.Id || (arg.Author.IsBot && inamiToken != arg.Author.Id)) // Inami is a bot used for integration testing
                return;
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            /// When playing games
            Task.Run(() => { gm.ReceiveMessageAsync(arg.Content, arg.Author, arg.Channel.Id); });

            int pos = 0;
            if (arg.Channel as ITextChannel == null) // The bot doesn't handle private messages
            {
                await arg.Channel.SendMessageAsync(Modules.Base.Sentences.DontPm(0));
                return;
            }
            string prefix = db.Prefixs[(arg.Channel as ITextChannel).GuildId]; // We get the bot prefix for this guild
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos) || (prefix != "" && msg.HasStringPrefix(prefix, ref pos)))
            {
                var context = new SocketCommandContext(client, msg);
                if (!((IGuildUser)await context.Channel.GetUserAsync(client.CurrentUser.Id)).GetPermissions((IGuildChannel)context.Channel).EmbedLinks)
                {
                    await context.Channel.SendMessageAsync(Modules.Base.Sentences.NeedEmbedLinks(context.Guild.Id));
                    return;
                }
                DateTime dt = DateTime.UtcNow;
                IResult result = await commands.ExecuteAsync(context, pos, null);
                if (result.IsSuccess && sendStats)
                {
                    await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("nbMsgs", "1") });
                    await AddError("OK");
                    await AddCommandServs(context.Guild.Id);
                }
            }
        }

        private async Task AddError(string name)
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("errors", name) });
        }

        private async Task AddCommandServs(ulong name)
        {
            await UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("commandServs", name.ToString()) });
        }

        private Task Log(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine(msg);
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }

        public Task LogError(LogMessage msg)
        {
            if (msg.Exception.InnerException != null && msg.Exception.InnerException.GetType() == typeof(NotAvailable))
            {
                CommandException ex = (CommandException)msg.Exception;
                ex.Context.Channel.SendMessageAsync(Modules.Base.Sentences.NotAvailable(ex.Context.Guild.Id));
                return Task.CompletedTask;
            }
            Log(msg);
            CommandException ce = msg.Exception as CommandException;
            if (ce != null)
            {
                if (ravenClient != null)
                    ravenClient.Capture(new SentryEvent(new Exception(msg.Message + Environment.NewLine + ce.Context.Message, msg.Exception)));
                ce.Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = msg.Exception.InnerException.GetType().ToString(),
                    Description = Modules.Base.Sentences.ExceptionThrown(ce.Context.Guild.Id, msg.Exception.InnerException.Message),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = Modules.Base.Sentences.ExceptionReported(ce.Context.Guild.Id)
                    }
                }.Build());
                if (sendStats)
                    AddError(msg.Exception.InnerException.GetType().ToString());
            }
            else
            {
                if (ravenClient != null)
                    ravenClient.Capture(new SentryEvent(msg.Exception));
                if (sendStats)
                    AddError(msg.Exception != null ? msg.Exception.Message.GetType().ToString() : "Unknown error");
            }
            return Task.CompletedTask;
        }
    }
}
