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
using System.Text.RegularExpressions;
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

        private List<int> commandModules; // Nb of command received per months (split by modules)
        public List<long> statsMonth; // Total size of download for boorus per months
        private int commandReceived; // Nb of command received per hours
        private string lastHourSent;
        public string lastMonthSent;

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

            #region StatsInit
            /// Stats at https://zirk.eu/sanara-stats.php
            lastHourSent = DateTime.Now.ToString("HH");
            lastMonthSent = DateTime.Now.ToString("MM");
            if (File.Exists("Saves/CommandReceived.dat"))
            {
                string[] content = File.ReadAllLines("Saves/CommandReceived.dat");
                if (content[1] == lastHourSent)
                    commandReceived = Convert.ToInt32(content[0]);
                else
                    commandReceived = 0;
            }
            else
                commandReceived = 0;

            commandModules = new List<int>();
            if (File.Exists("Saves/CommandModules.dat"))
            {
                string[] content = File.ReadAllLines("Saves/CommandModules.dat");
                if (content[1] == lastHourSent)
                {
                    string[] mods = content[0].Split('|');
                    for (int i = 0; i <= (int)Module.Youtube; i++)
                        commandModules.Add(Convert.ToInt32(mods[i]));
                }
                else
                    for (int i = 0; i <= (int)Module.Youtube; i++)
                        commandModules.Add(0);
            }
            else
                for (int i = 0; i <= (int)Module.Youtube; i++)
                    commandModules.Add(0);

            statsMonth = new List<long>();
            if (File.Exists("Saves/MonthModules.dat"))
            {
                string[] content = File.ReadAllLines("Saves/MonthModules.dat");
                if (content[1] == lastMonthSent)
                {
                    string[] mods = content[0].Split('|');
                    for (int i = 0; i < 4; i++)
                        statsMonth.Add(Convert.ToInt64(mods[i]));
                }
                else
                    for (int i = 0; i < 4; i++)
                        statsMonth.Add(0);
            }
            else
                for (int i = 0; i < 4; i++)
                    statsMonth.Add(0);
            #endregion StatsInit

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

        /// <summary>
        /// Get a user by his username/nickname/id
        /// </summary>
        /// <param name="name">The name/id of the user</param>
        /// <param name="guild">The guild the user is in</param>
        /// <returns></returns>
        public static async Task<IGuildUser> GetUser(string name, IGuild guild)
        {
            Match match = Regex.Match(name, "<@[!]?[0-9]{18}>");
            if (match.Success)
            {
                try
                {
                    string val = "";
                    foreach (char c in match.Value)
                    {
                        if (char.IsNumber(c))
                            val += c;
                    }
                    return (await guild.GetUserAsync(Convert.ToUInt64(val)));
                }
                catch (Exception)
                { }
            }
            try
            {
                return (await guild.GetUserAsync(Convert.ToUInt64(name)));
            }
            catch (Exception)
            { }
            foreach (IGuildUser user in await guild.GetUsersAsync())
            {
                if (user.Nickname == name || user.Username == name)
                    return (user);
            }
            return (null);
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

        /// <summary>
        /// Get usage of modules for the current month and amount of users by servers
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetModulesStats()
        {
            string currDate = DateTime.Now.ToString("yyyyMM");
            List<string> allModules = new List<string>();
            int[] valuesModules = new int[(int)Module.Youtube + 1];
            for (int i = 0; i < valuesModules.Length; i++)
            {
                allModules.Add(((Module)i).ToString().ToLower());
                valuesModules[i] = 0;
            }
            foreach (string d in Directory.GetDirectories("Saves/Servers"))
            {
                string dir = d.Replace('\\', '/') + "/ModuleCount/" + currDate;
                if (Directory.Exists(dir))
                {
                    foreach (string f in Directory.GetFiles(dir))
                    {
                        string[] elems = f.Split(new string[] { "/", "\\" }, StringSplitOptions.None);
                        int index = allModules.ToList().FindIndex(x => elems[elems.Length - 1].ToLower().Contains(x));
                        if (index > -1)
                            valuesModules[index] += Convert.ToInt32(File.ReadAllText(f));
                    }
                }
            }
            string finalStr = "";
            for (int i = 0; i < valuesModules.Length; i++)
                finalStr += valuesModules[i] + "|";
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
            while (guilds.Count > 0)
            {
                foreach (var tuple in guilds)
                {
                    if (biggest == null || tuple.Item2 > biggest.Item2)
                        biggest = tuple;
                }
                finalStr += biggest.Item1.Replace("'", "") + "|" + biggest.Item2 + "|" + biggest.Item3 + "|";
                guilds.Remove(biggest);
                biggest = null;
            }
            return (finalStr);
        }

        public enum Module
        {
            AnimeManga,
            Booru,
            Code,
            Communication,
            Debug,
            Doujinshi,
            Game,
            GoogleShortener,
            Kancolle,
            Linguistic,
            Radio,
            Settings,
            Vn,
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
            if (!u.IsBot)
            {
                commandModules[(int)m]++;
                string finalStr = "";
                foreach (int i in commandModules)
                    finalStr += i + "|";
                finalStr = finalStr.Substring(0, finalStr.Length - 1);
                File.WriteAllText("Saves/CommandModules.dat", finalStr + Environment.NewLine + lastHourSent);
            }
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
        /// <returns>the size of the folder</returns>
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
            if (lastHourSent != DateTime.Now.ToString("HH"))
            {
                lastHourSent = DateTime.Now.ToString("HH");
                commandReceived = 0;
                for (int i = 0; i <= (int)Module.Youtube; i++)
                    commandModules[i] = 0;
                values.Add("modules", await GetModulesStats());
            }
            if (lastMonthSent != DateTime.Now.ToString("MM"))
            {
                lastHourSent = DateTime.Now.ToString("MM");
                for (int i = 0; i < 4; i++)
                    statsMonth[i] = 0;
            }
            string finalStr = "";
            foreach (int i in commandModules)
                finalStr += i + "|";
            string finalStrMonth = "";
            foreach (int i in statsMonth)
                finalStrMonth += i + "|";
            values.Add("nbMsgs", commandReceived.ToString());
            values.Add("serverModules", finalStr);
            values.Add("monthStats", finalStrMonth);
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
                    commandReceived++;
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
                    File.WriteAllText("Saves/CommandReceived.dat", commandReceived + Environment.NewLine + lastHourSent);
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
