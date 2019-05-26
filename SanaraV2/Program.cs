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
using SharpRaven;
using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static SanaraV2.Modules.Entertainment.RadioModule;

namespace SanaraV2
{
    public class Program
    {
        public static async Task Main()
        {
            try
            {
                await new Program().MainAsync();
            }
            catch (FileNotFoundException e) // This probably means a dll is missing
            {
                throw e;
            }
            catch (Exception e) // If an exception occur, the program exit and is relaunched
            {
                if (Debugger.IsAttached)
                    throw e;
            }
        }

        public readonly DiscordSocketClient client;
        private readonly CommandService commands = new CommandService();
        public static Program p;
        public Random rand;

        public GameManager gm;

        private GoogleCredential credential;
        public TranslationClient translationClient;

        public YouTubeService youtubeService;

        private string websiteStats, websiteStatsToken;
        private AuthDiscordBotListApi dblApi;

        public List<RadioChannel> radios;

        public DateTime startTime;

        public Dictionary<string, List<Translation.TranslationData>> translations;
        public Dictionary<string, List<string>> allLanguages;

        private RavenClient ravenClient;
        public ImageAnnotatorClient visionClient;
        public bool sendStats { private set; get; }

        private DateTime lastDiscordBotsSent; // We make sure to wait at least 10 mins before sending stats to DiscordBots.org so we don't spam the API

        public Db.Db db;

        private ulong inamiToken;

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
            inamiToken = inamiId;

            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            db = new Db.Db();
            await db.InitAsync();

            gm = new GameManager();

            p = this;

            rand = new Random();

            UpdateLanguageFiles();
            if (botToken == null && !File.Exists("Keys/Credentials.json"))
                throw new FileNotFoundException("You must have a Credentials.json file located in " + AppDomain.CurrentDomain.BaseDirectory + "Keys, more information at https://sanara.zirk.eu/documentation.html?display=Clone");
            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText("Keys/Credentials.json"));
            if (botToken == null && (json.botToken == null || json.ownerId == null || json.ownerStr == null))
                throw new NullReferenceException("Your Credentials.json is missing mandatory information, it must at least contains botToken, ownerId and ownerStr. More information at https://sanara.zirk.eu/documentation.html?display=Clone");
            Modules.Base.Sentences.ownerId = ulong.Parse((string)json.ownerId);
            Modules.Base.Sentences.ownerStr = json.ownerStr;

            websiteStats = json.websiteStats;
            websiteStatsToken = json.websiteStatsToken;
            sendStats = websiteStats != null && websiteStatsToken != null;
            if (json.discordBotsId != null && json.discordBotsToken != null)
                dblApi = new AuthDiscordBotListApi(ulong.Parse((string)json.discordBotsId), (string)json.discordBotsToken);
            else

                dblApi = null;
            await InitServices(json);
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
            await commands.AddModuleAsync<Modules.Tools.Image>(null);
            await commands.AddModuleAsync<Communication>(null);

            client.MessageReceived += HandleCommandAsync;
            client.GuildAvailable += GuildJoin;
            client.JoinedGuild += GuildJoin;
            client.JoinedGuild += GuildCountChange;
            client.LeftGuild += GuildCountChange;
            client.Disconnected += Disconnected;
            client.UserVoiceStateUpdated += VoiceUpdate;
            client.Connected += UpdateDiscordBots;

            await client.LoginAsync(TokenType.Bot, (botToken == null) ? (string)json.botToken : botToken);
            startTime = DateTime.Now;
            await client.StartAsync();

            if (sendStats)
            {
                var task = Task.Run(async () =>
                {
                    for (;;)
                    {
                        await Task.Delay(60000);
                        UpdateStatus();
                    }
                });
            }

            if (botToken == null) // Unit test manage the bot life
                await Task.Delay(-1);
        }

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

        private async Task VoiceUpdate(SocketUser user, SocketVoiceState state, SocketVoiceState state2)
        {
            RadioChannel radio = radios.Find(x => x.m_guildId == ((IGuildUser)user).GuildId);
            if (radio != null && await radio.IsChanEmpty())
            {
                await radio.Stop();
                radios.Remove(radio);
            }
        }

        private Task Disconnected(Exception e)
        {
            Environment.Exit(1);
            return Task.CompletedTask;
        }

        private async Task InitServices(dynamic json)
        {
            if (File.Exists("youtube-dl.exe"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("youtube-dl.exe", "-U");
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                Process process = Process.Start(startInfo);
                await Log(new LogMessage(LogSeverity.Info, "Setup", await process.StandardOutput.ReadToEndAsync()));
            }

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

            radios = new List<RadioModule.RadioChannel>();

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
        }

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
            Image,
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
            catch (HttpRequestException)
            { }
            catch (TaskCanceledException)
            { }
        }

        /// Clean name before sending it to the website for stats (| and & are delimitators so we remove them)
        public string GetName(string name)
            => name.Replace("|", "").Replace("$", "");

        private async void UpdateStatus()
        {
            // Server count
            List<Tuple<string, int, int>> guilds = new List<Tuple<string, int, int>>();
            foreach (IGuild g in client.Guilds)
            {
                int users = 0;
                int bots = 0;
                foreach (IGuildUser u in await g.GetUsersAsync())
                {
                    if (u.IsBot) bots++;
                    else users++;
                }
                guilds.Add(new Tuple<string, int, int>(g.Name, users, bots));
            }

            // Server biggest
            Tuple<string, int, int> biggest = null;
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
                finalStr += GetName(biggest.Item1) + "|" + biggest.Item2 + "|" + biggest.Item3 + "$";
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
            if (arg.Channel as ITextChannel == null)
            {
                await arg.Channel.SendMessageAsync(Modules.Base.Sentences.DontPm(0));
                return;
            }
            string prefix = db.Prefixs[(arg.Channel as ITextChannel).GuildId];
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
                    Description = Modules.Base.Sentences.ExceptionThrown(ce.Context.Guild.Id, msg.Exception.InnerException.Message)
                }.Build());
                if (sendStats)
                    AddError(msg.Exception.InnerException.GetType().ToString());
            }
            else
            {
                if (ravenClient != null)
                    ravenClient.Capture(new SentryEvent(msg.Exception));
                if (sendStats)
                    AddError("Unknown error");
            }
            return Task.CompletedTask;
        }
    }
}
