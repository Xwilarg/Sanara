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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SanaraV2
{
    partial class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public readonly DiscordSocketClient client;
        private readonly IServiceCollection map = new ServiceCollection();
        private readonly CommandService commands = new CommandService();
        public static Program p;
        public Random rand;

        public List<GameModule.Game> games;
        public Thread gameThread;

        public WebClient malClient;

        public List<Character> relations;

        private GoogleCredential credential;
        public TranslationClient translationClient;

        public YouTubeService youtubeService;

        public List<RadioModule.RadioChannel> radios;

        public UrlshortenerService service;

        private Program()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
            });
            client.Log += Log;
            commands.Log += Log;
        }

        private async Task MainAsync()
        {
            p = this;
            games = new List<GameModule.Game>();
            gameThread = new Thread(new ThreadStart(GameThread));

            rand = new Random();
            relations = new List<Character>();

            string[] malCredentials = File.ReadAllLines("Keys/malPwd.dat");
            malClient = new WebClient();
            malClient.Credentials = new NetworkCredential(malCredentials[0], malCredentials[1]);

            credential = GoogleCredential.FromFile(@"Keys\Sanara-7430da57d6af.json");
            translationClient = TranslationClient.Create(credential);

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = File.ReadAllText("Keys/YoutubeAPIKey.dat")
            });

            radios = new List<RadioModule.RadioChannel>();

            service = new UrlshortenerService(new BaseClientService.Initializer
            {
                ApiKey = File.ReadAllText("Keys/URLShortenerAPIKey.dat"),
            });

            await commands.AddModuleAsync<CommunicationModule>();
            await commands.AddModuleAsync<SettingsModule>();
            await commands.AddModuleAsync<LinguistModule>();
            await commands.AddModuleAsync<KancolleModule>();
            await commands.AddModuleAsync<DebugModule>();
            await commands.AddModuleAsync<BooruModule>();
            await commands.AddModuleAsync<VndbModule>();
            await commands.AddModuleAsync<NhentaiModule>();
            await commands.AddModuleAsync<CodeModule>();
            await commands.AddModuleAsync<MyAnimeListModule>();
            await commands.AddModuleAsync<GameModule>();
            await commands.AddModuleAsync<YoutubeModule>();
            await commands.AddModuleAsync<GoogleShortenerModule>();
            await commands.AddModuleAsync<RadioModule>();

            client.MessageReceived += HandleCommandAsync;
            client.GuildAvailable += GuildJoin;
            client.UserJoined += UserJoin;
            client.JoinedGuild += GuildJoin;

            await client.LoginAsync(TokenType.Bot, File.ReadAllText("Keys/token.dat"));
            await client.StartAsync();

            var task = Task.Run(async () => {
                for (;;)
                {
                    await Task.Delay(60000);
                    UpdateStatus();
                }
            });

            await Task.Delay(-1);
        }

        /// <summary>
        /// When receiving string from website, sometimes you have to replace some stuffs on them.
        /// </summary>
        /// <param name="text">The string to deal with</param>
        /// <returns></returns>
        public static string removeUnwantedSymboles(string text)
        {
            text = text.Replace("[i]", "*");
            text = text.Replace("[/i]", "*");
            text = text.Replace("&lt;br /&gt;", Environment.NewLine);
            text = text.Replace("mdash;", "—");
            text = text.Replace("&quot;", "\"");
            text = text.Replace("&amp;", "&");
            text = text.Replace("&#039;", "'");
            return (text);
        }

        /// <summary>
        /// Every commands take a string[] in parameter so they can be called with any number of arguments.
        /// This function transform it to a string adding spaces between each elements of the array
        /// </summary>
        /// <param name="args">The string[] to deal with</param>
        /// <returns></returns>
        public static string addArgs(string[] args)
        {
            if (args.Length == 0)
                return (null);
            string finalStr = args[0];
            for (int i = 1; i < args.Length; i++)
            {
                finalStr += " " + args[i];
            }
            return (finalStr);
        }

        /// <summary>
        /// For comparaisons between 2 string it's sometimes useful that you remove everything except number and letters
        /// </summary>
        /// <param name="word">The string to deal with</param>
        /// <returns></returns>
        public static string cleanWord(string word)
        {
            string finalStr = "";
            foreach (char c in word)
            {
                if (char.IsLetterOrDigit(c))
                    finalStr += char.ToUpper(c);
            }
            return (finalStr);
        }

        /// <summary>
        /// Get an element in a string
        /// </summary>
        /// <param name="tag">The tag where we begin to take the element</param>
        /// <param name="file">The string to search in</param>
        /// <param name="stopCharac">The character after with we stop looking for</param>
        /// <returns></returns>
        public static string getElementXml(string tag, string file, char stopCharac)
        {
            string saveString = "";
            int prog = 0;
            char lastChar = ' ';
            foreach (char c in file)
            {
                if (prog == tag.Length)
                {
                    if (c == stopCharac
                        && ((stopCharac == '"' && lastChar != '\\') || stopCharac != '"'))
                        break;
                    saveString += c;
                }
                else
                {
                    if (c == tag[prog])
                        prog++;
                    else
                        prog = 0;
                }
                lastChar = c;
            }
            return (saveString);
        }

        private async Task GuildJoin(SocketGuild arg)
        {
            string currTime = DateTime.UtcNow.ToString("ddMMyyHHmmss");
            ITextChannel chan = returnChannel(arg.Channels.ToList(), arg.Id);
            if (!Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");
            if (!File.Exists("Saves/sanaraDatas.dat"))
                File.WriteAllText("Saves/sanaraDatas.dat", currTime); // Creation date
            if (!Directory.Exists("Saves/Servers/" + arg.Id))
            {
                Directory.CreateDirectory("Saves/Servers/" + arg.Id);
                File.WriteAllText("Saves/Servers/" + arg.Id + "/serverDatas.dat", currTime + Environment.NewLine + 0 + Environment.NewLine + arg.Name); // Join date | unused | server name
                await chan.SendMessageAsync(Sentences.introductionMsg);
            }
            if (!File.Exists("Saves/Servers/" + arg.Id + "/kancolle.dat"))
                File.WriteAllText("Saves/Servers/" + arg.Id + "/kancolle.dat", "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0" + Environment.NewLine + "0");
            // Attempt game, attempt ship, ship found, bestScore, ids of people who help to have the best score
            if (!Directory.Exists("Saves/Users"))
                Directory.CreateDirectory("Saves/Users");
            foreach (IUser u in arg.Users)
            {
                if (!File.Exists("Saves/Users/" + u.Id + ".dat"))
                {
                    relations.Add(new Character(u.Id, u.Username));
                }
                else
                {
                    try
                    {
                        if (!relations.Any(x => x._name == Convert.ToUInt64(File.ReadAllLines("Saves/Users/" + u.Id + ".dat")[1])))
                        {
                            relations.Add(new Character());
                            relations[relations.Count - 1].saveAndParseInfos(File.ReadAllLines("Saves/Users/" + u.Id + ".dat"));
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        if (arg.Id.ToString() == File.ReadAllLines("Saves/sanaraDatas.dat")[2])
                        {
                            await chan.SendMessageAsync(Sentences.introductionError(u.Id.ToString(), u.Username));
                        }
                    }
                }
            }
        }

#pragma warning disable CS1998
        private async Task UserJoin(SocketGuildUser arg)
        {
            string currTime = DateTime.UtcNow.ToString("ddMMyyHHmmss");
            if (!File.Exists("Saves/Users/" + arg.Id + ".dat"))
            {
                relations.Add(new Character(arg.Id, arg.Nickname));
            }
        }
#pragma warning restore CS1998

        public enum Module
        {
            Booru,
            Code,
            Communication,
            Debug,
            Game,
            GoogleShortener,
            Jisho,
            Kancolle,
            MyAnimeList,
            Nhentai,
            Radio,
            Settings,
            Vndb,
            Youtube
        }

        /// <summary>
        /// Each commands call this functions that record datas about them
        /// </summary>
        /// <param name="u">User that sent the message</param>
        /// <param name="serverId">The ID of the current guild</param>
        /// <param name="m">The module that was called (see above)</param>
        public void doAction(IUser u, ulong serverId, Module m)
        {
            if (!Directory.Exists("Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM")))
            {
                Directory.CreateDirectory("Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM"));
            }
            for (int i = 0; i <= (int)Module.Youtube + 1; i++)
            {
                string filePath = "Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM") + "/"
                    + ((Module)i).ToString()[0] + ((Module)i).ToString().ToLower().Substring(1, ((Module)i).ToString().Length - 1) + ".dat";
                if (!File.Exists(filePath))
                    File.WriteAllText(filePath, "0");
            }
            string finalFilePath = "Saves/Servers/" + serverId + "/ModuleCount/" + DateTime.UtcNow.ToString("yyyyMM") + "/"
                        + m.ToString()[0] + m.ToString().ToLower().Substring(1, m.ToString().Length - 1) + ".dat";
            File.WriteAllText(finalFilePath,
                        (Convert.ToInt32(File.ReadAllText(finalFilePath)) + 1).ToString());
            foreach (Character c in relations)
            {
                if (u.Id == c._name)
                {
                    c.increaseNbMessage();
                    c.meet();
                    break;
                }
            }
        }

        /// <summary>
        /// Get size of the given folder
        /// </summary>
        /// <param name="folder">folder which size need to be calculated</param>
        /// <returns></returns>
        private long getLenghtFolder(string folder)
        {
            long currSize = 0;
            foreach (string s in Directory.GetFiles(folder))
            {
                FileInfo fi = new FileInfo(s);
                currSize += fi.Length;
            }
            foreach (string s in Directory.GetDirectories(folder))
            {
                currSize += getLenghtFolder(s);
            }
            return (currSize);
        }

        private async void UpdateStatus()
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string> {
                           { "token", File.ReadAllLines("Keys/websiteToken.dat")[1] },
                           { "name", "Sanara" }
                        };
            FormUrlEncodedContent content = new FormUrlEncodedContent(values);

            try
            {
                await client.PostAsync(File.ReadAllLines("Keys/websiteToken.dat")[0], content);
            }
            catch (HttpRequestException)
            { }
            catch (TaskCanceledException)
            { }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;
            if (arg.Author.Id == 352216646267437059)
            {
                /// Get some informations about the bot for statistics purposes
                if (arg.Content == "<@&330483872544587776> Please send me your informations for comparison.")
                {
                    List<ulong> users = new List<ulong>();
                    foreach (var g in client.Guilds)
                    {
                        foreach (var g2 in g.Users)
                        {
                            if (!users.Contains(g2.Id))
                            {
                                users.Add(g2.Id);
                            }
                        }
                    }
                    int nbMsgs = 0;
                    DateTime dt = DateTime.UtcNow;
                    if (Directory.Exists("Saves/Stats/" + dt.Month.ToString()))
                    {
                        foreach (string s in Directory.GetFiles("Saves/Stats/" + dt.Month.ToString()))
                        {
                            nbMsgs += Convert.ToInt32(File.ReadAllText(s));
                        }
                    }
                    await (((ITextChannel)(arg.Channel))).SendMessageAsync($"<@{Sentences.idPikyu}> Here are the compare informations: " + client.Guilds.Count + "|" + users.Count + "|" + getLenghtFolder("Saves") + "o|Zirk|" + nbMsgs);
                }
            }
            /// When playing games
            else if (arg.Author.Id != Sentences.myId)
            {
                GameModule.Game game = games.Find(x => x.m_chan == arg.Channel);
                if (game != null)
                    game.CheckCorrect(arg.Content, arg.Author);
            }
            int pos = 0;
            if (msg.HasMentionPrefix(client.CurrentUser, ref pos))
            {
                var context = new SocketCommandContext(client, msg);
                DateTime dt = DateTime.UtcNow;
                var result = await commands.ExecuteAsync(context, pos);
                // Count how many messages the bot receive
                if (result.IsSuccess && !context.User.IsBot)
                {
                    if (!Directory.Exists("Saves"))
                        Directory.CreateDirectory("Saves");
                    if (!Directory.Exists("Saves/Stats"))
                        Directory.CreateDirectory("Saves/Stats");
                    if (!Directory.Exists("Saves/Stats/" + dt.Month.ToString()))
                        Directory.CreateDirectory("Saves/Stats/" + dt.Month.ToString());
                    if (File.Exists("Saves/Stats/" + dt.Month.ToString() + '/' + dt.Day.ToString() + ".dat"))
                        File.WriteAllText("Saves/Stats/" + dt.Month.ToString() + '/' + dt.Day.ToString() + ".dat", (Convert.ToInt32(File.ReadAllText("Saves/Stats/" + dt.Month.ToString() + '/' + dt.Day.ToString() + ".dat")) + 1).ToString());
                    else
                        File.WriteAllText("Saves/Stats/" + dt.Month.ToString() + '/' + dt.Day.ToString() + ".dat", "1");
                }
            }
        }

        /// <summary>
        /// Return the first text channel the bot find on the guild.
        /// Used when she greet people
        /// </summary>
        /// <param name="s"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        private ITextChannel returnChannel(List<SocketGuildChannel> s, ulong serverId)
        {
            foreach (IChannel c in s)
            {
                if (c.GetType().Name == "SocketTextChannel")
                {
                    if ((!Directory.Exists("Saves/Servers/" + serverId))
                    || (Directory.Exists("Saves/Servers/" + serverId) && c.Id == Convert.ToUInt64(File.ReadAllLines("Saves/Servers/" + serverId + "/serverDatas.dat")[1])))
                    {
                        return (ITextChannel)(c);
                    }
                }
            }
            foreach (IChannel c in s)
            {
                if (c.GetType().Name == "SocketTextChannel")
                {
                    return (ITextChannel)(c);
                }
            }
            return (null);
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
    }
}
