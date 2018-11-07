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
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Google.Apis.YouTube.v3;
using Google.Cloud.Translation.V2;
using Google.Cloud.Vision.V1;
using Microsoft.Extensions.DependencyInjection;
using SanaraV2.Modules.Base;
using SanaraV2.Modules.Entertainment;
using SanaraV2.Modules.GamesInfo;
using SanaraV2.Modules.NSFW;
using SanaraV2.Modules.Tools;
using SharpRaven;
using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync(true).GetAwaiter().GetResult();

        public readonly DiscordSocketClient client;
        private readonly IServiceCollection map = new ServiceCollection();
        private readonly CommandService commands = new CommandService();
        public static Program p;
        public Random rand;

        public List<GameModule.Game> games;
        public Thread gameThread;

        private GoogleCredential credential;
        public TranslationClient translationClient;

        public YouTubeService youtubeService;

        public List<RadioModule.RadioChannel> radios;

        public UrlshortenerService service;

        public DateTime startTime;

        public Dictionary<string, List<Translation.TranslationData>> translations;
        public Dictionary<ulong, string> guildLanguages;
        public Dictionary<string, List<string>> allLanguages;

        public Dictionary<ulong, string> prefixs;

        private RavenClient ravenClient;
        public ImageAnnotatorClient visionClient;
        public bool sendStats { private set; get; }

        private Program()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Log;
            commands.Log += LogError;
        }

        public Program(bool unused) // For unit tests
        {
            MainAsync(false).GetAwaiter().GetResult();
        }

        private async Task MainAsync(bool launchBot)
        {
            p = this;
            games = new List<GameModule.Game>();
            gameThread = new Thread(new ThreadStart(GameThread));

            rand = new Random();

            UpdateLanguageFiles();
            guildLanguages = new Dictionary<ulong, string>();

            prefixs = new Dictionary<ulong, string>();

            sendStats = File.Exists("Keys/websiteToken.dat");
            InitServices();

            if (!launchBot)
                return;

            await commands.AddModuleAsync<Communication>();
            await commands.AddModuleAsync<Settings>();
            await commands.AddModuleAsync<Linguist>();
            await commands.AddModuleAsync<Kancolle>();
            await commands.AddModuleAsync<Booru>();
            await commands.AddModuleAsync<VnModule>();
            await commands.AddModuleAsync<Doujinshi>();
            await commands.AddModuleAsync<Code>();
            await commands.AddModuleAsync<AnimeManga>();
            await commands.AddModuleAsync<GameModule>();
            await commands.AddModuleAsync<YoutubeModule>();
            await commands.AddModuleAsync<GoogleShortener>();
            await commands.AddModuleAsync<RadioModule>();
            await commands.AddModuleAsync<XKCD>();
            await commands.AddModuleAsync<Modules.Tools.Image>();
            await commands.AddModuleAsync<GirlsFrontline>();

            client.MessageReceived += HandleCommandAsync;
            client.GuildAvailable += GuildJoin;
            client.JoinedGuild += GuildJoin;
            client.Disconnected += Disconnected;

            await client.LoginAsync(TokenType.Bot, File.ReadAllText("Keys/token.dat"));
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
            await Task.Delay(-1);
        }

        private Task Disconnected(Exception e)
        {
            Environment.Exit(1);
            return Task.CompletedTask;
        }
        
        private void InitServices()
        {
            if (File.Exists("Keys/Sanara-7430da57d6af.json"))
            {
                credential = GoogleCredential.FromFile("Keys/Sanara-7430da57d6af.json");
                translationClient = TranslationClient.Create(credential);
            }
            else
                translationClient = null;

            if (File.Exists("Keys/YoutubeAPIKey.dat"))
            {
                youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = File.ReadAllText("Keys/YoutubeAPIKey.dat")
                });
            }
            else
                youtubeService = null;

            radios = new List<RadioModule.RadioChannel>();

            if (File.Exists("Keys/URLShortenerAPIKey.dat"))
            {
                service = new UrlshortenerService(new BaseClientService.Initializer
                {
                    ApiKey = File.ReadAllText("Keys/URLShortenerAPIKey.dat"),
                });
            }
            else
                service = null;

            if (File.Exists("Keys/raven.dat"))
            {
                ravenClient = new RavenClient(File.ReadAllText("Keys/raven.dat"));
            }
            else
                ravenClient = null;
            
            if (File.Exists("Keys/visionAPI.json"))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Keys/visionAPI.json");
                visionClient = ImageAnnotatorClient.Create();
            }
            else
                visionClient = null;
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
            string currTime = DateTime.UtcNow.ToString("ddMMyyHHmmss");
            if (!Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");
            if (!File.Exists("Saves/sanaraDatas.dat"))
                Utilities.WriteAllText("Saves/sanaraDatas.dat", currTime); // Creation date
            if (!Directory.Exists("Saves/Servers"))
                Directory.CreateDirectory("Saves/Servers");
            if (!Directory.Exists("Saves/Servers/" + arg.Id))
            {
                Directory.CreateDirectory("Saves/Servers/" + arg.Id);
                Utilities.WriteAllText("Saves/Servers/" + arg.Id + "/serverDatas.dat", currTime + Environment.NewLine + 0 + Environment.NewLine + arg.Name); // Join date | unused | server name
            }
            if (!Directory.Exists("Saves/Users"))
                Directory.CreateDirectory("Saves/Users");
            guildLanguages.Add(arg.Id, (File.Exists("Saves/Servers/" + arg.Id + "/language.dat")) ? (File.ReadAllText("Saves/Servers/" + arg.Id + "/language.dat")) : ("en"));
            prefixs.Add(arg.Id, (File.Exists("Saves/Servers/" + arg.Id + "/prefix.dat")) ? (File.ReadAllText("Saves/Servers/" + arg.Id + "/prefix.dat")) : ("s."));
        }

        public enum Module
        {
            AnimeManga,
            Booru,
            Code,
            Communication,
            Doujinshi,
            Game,
            GirlsFrontier,
            GoogleShortener,
            Image,
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
            {
                await UpdateElement(new Tuple<string, string>[] {   new Tuple<string, string>("modules", m.ToString()) });
            }
            DateTime now = DateTime.UtcNow;
            if (!Directory.Exists("Saves/Servers/" + serverId + "/ModuleCount/" + now.ToString("yyyyMM")))
            {
                Directory.CreateDirectory("Saves/Servers/" + serverId + "/ModuleCount/" + now.ToString("yyyyMM"));
            }
            for (int i = 0; i <= (int)Module.Youtube + 1; i++)
            {
                string filePath = "Saves/Servers/" + serverId + "/ModuleCount/" + now.ToString("yyyyMM") + "/"
                    + ((Module)i).ToString()[0] + ((Module)i).ToString().ToLower().Substring(1, ((Module)i).ToString().Length - 1) + ".dat";
                if (!File.Exists(filePath))
                    File.WriteAllText(filePath, "0");
            }
            string finalFilePath = "Saves/Servers/" + serverId + "/ModuleCount/" + now.ToString("yyyyMM") + "/"
                        + m.ToString()[0] + m.ToString().ToLower().Substring(1, m.ToString().Length - 1) + ".dat";
            File.WriteAllText(finalFilePath,
                        (Convert.ToInt32(File.ReadAllText(finalFilePath)) + 1).ToString());
            bool didFound = false;
            if (!didFound)
            {
                Character charac = new Character(u.Id, u.Username);
                charac.IncreaseNbMessage();
            }
        }

        public async Task UpdateElement(Tuple<string, string>[] elems)
        {
            HttpClient httpClient = new HttpClient();
            var values = new Dictionary<string, string> {
                           { "token", File.ReadAllLines("Keys/websiteToken.dat")[1] },
                           { "action", "add" },
                           { "name", "Sanara" }
                        };
            foreach (var elem in elems)
            {
                values.Add(elem.Item1, elem.Item2);
            }
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, File.ReadAllLines("Keys/websiteToken.dat")[0]);
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

        private async void UpdateStatus()
        {
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
                finalStr += biggest.Item1.Replace("|", "").Replace("$", "") + "|" + biggest.Item2 + "|" + biggest.Item3 + "$";
                guilds.Remove(biggest);
                biggest = null;
            }
            await UpdateElement(new Tuple<string, string>[] {   new Tuple<string, string>("serverCount", client.Guilds.Count.ToString()),
                                                                new Tuple<string, string>("serversBiggest", finalStr) });
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg.Author.Id == Modules.Base.Sentences.myId || arg.Author.IsBot)
                return;
            var msg = arg as SocketUserMessage;
            if (msg == null) return;
            /// When playing games
            else if (arg.Author.Id != Modules.Base.Sentences.myId)
            {
                GameModule.Game game = games.Find(x => x.m_chan == arg.Channel);
                if (game != null)
                    await game.CheckCorrect(arg.Content, arg.Author);
            }
            int pos = 0;
            string prefix = prefixs[(arg.Channel as ITextChannel).GuildId];
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos) || (prefix != "" && msg.HasStringPrefix(prefix, ref pos)))
            {
                var context = new SocketCommandContext(client, msg);
                if (context.Guild == null)
                {
                    await context.Channel.SendMessageAsync(Modules.Base.Sentences.DontPm(0));
                    return;
                }
                DateTime dt = DateTime.UtcNow;
                var result = await commands.ExecuteAsync(context, pos);
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

        public void GameThread()
        {
            while (Thread.CurrentThread.IsAlive) // TODO: Replace thread with async
            {
                try
                {
                    for (int i = p.games.Count - 1; i >= 0; i--)
                    {
                        if (p.games[i].IsGameLost())
                        {
                            p.games[i].Loose();
                            p.games.RemoveAt(i);
                        }
                    }
                }
                catch (InvalidOperationException)
                { }
                Thread.Sleep(100);
            }
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

        private Task LogError(LogMessage msg)
        {
            Log(msg);
            if (ravenClient != null)
                ravenClient.Capture(new SentryEvent(msg.Exception));
            CommandException ce = msg.Exception as CommandException;
            if (ce != null)
            {
                ce.Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = msg.Exception.InnerException.GetType().ToString(),
                    Description = Modules.Base.Sentences.ExceptionThrown(ce.Context.Guild.Id, msg.Exception.InnerException.Message)
                }.Build());
                if (sendStats)
                    AddError(msg.Exception.InnerException.GetType().ToString());
            }
            else if (sendStats)
                AddError("Unknown error");
            return Task.CompletedTask;
        }
    }
}
