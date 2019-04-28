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
using BooruSharp.Booru;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using SanaraV2.Modules.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Entertainment
{
    public class GameModule : ModuleBase
    {
        Program p = Program.p;

        public static readonly int shiritoriTimer = 15;
        public static readonly int kancolleTimer = 15;
        public static readonly int booruTimer = 45;
        public static readonly int animeTimer = 30;
        public static readonly int azurlaneTimer = 15;

        public abstract class Game
        {
            protected Game(IMessageChannel chan, IGuild guild, IUser charac, int refTime, string fileName, bool isEasy, bool isFull)
            {
                m_chan = (ITextChannel)chan;
                m_didLost = false;
                m_refTime = refTime * ((isEasy) ? (2) : (1));
                m_time = DateTime.Now;
                m_guild = guild;
                m_nbAttempt = 0;
                m_nbFound = 0;
                m_userIds = new List<ulong>();
                m_fileName = fileName + ((isEasy) ? ("-easy") : ("")) + ((isFull) ? ("-full") : (""));
            }

            public bool IsGameLost()
            {
                return (m_didLost || (m_time != DateTime.MinValue && m_time.AddSeconds(m_refTime).CompareTo(DateTime.Now) == -1));
            }

            protected async void SaveServerScores(string answer)
            {
                var newScore = await Program.p.db.SetNewScore(m_fileName, m_nbFound, m_guild.Id, string.Join("|", m_userIds));
                string finalStr = (answer == null) ? ("") : (Sentences.TimeoutGame(m_guild.Id, answer) + Environment.NewLine);
                if (newScore.Item1 == Db.Db.Comparaison.Best)
                    finalStr += Sentences.NewBestScore(m_guild.Id, newScore.Item2.ToString(), m_nbFound.ToString());
                else if (newScore.Item1 == Db.Db.Comparaison.Equal)
                    finalStr += Sentences.EqualizedScore(m_guild.Id, m_nbFound.ToString());
                else
                    finalStr += Sentences.DidntBeatScore(m_guild.Id, newScore.Item2.ToString(), m_nbFound.ToString());
                await m_chan.SendMessageAsync(finalStr);
            }

            public async Task Post(int counter = 1)
            {
                m_time = DateTime.MinValue;
                {
                    bool isImage = IsPostImage();
                    foreach (string msg in GetPost())
                    {
                        if (!isImage)
                            await m_chan.SendMessageAsync(msg);
                        else
                        {
                            try
                            {
                                using (HttpClient hc = new HttpClient())
                                {
                                    Stream s = await hc.GetStreamAsync(msg);
                                    Stream s2 = await hc.GetStreamAsync(msg); // We create a new Stream because the first one is altered.
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        await s.CopyToAsync(ms);
                                        if (ms.ToArray().Length < 8000000)
                                            await m_chan.SendFileAsync(s2, "Sanara-image." + msg.Split('.').Last());
                                        else
                                            await m_chan.SendMessageAsync(msg);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                string exceptionMsg = (counter < 2) ? Sentences.ExceptionGame(m_guild.Id, msg) : Sentences.ExceptionGameStop(m_guild.Id);
                                await m_chan.SendMessageAsync("", false, new EmbedBuilder()
                                {
                                    Color = Color.Red,
                                    Title = e.GetType().ToString(),
                                    Description = exceptionMsg,
                                    Footer = new EmbedFooterBuilder()
                                    {
                                        Text = e.Message
                                    }
                                }.Build());
                                if (counter < 2)
                                {
                                    await Post(counter + 1);
                                    return;
                                }
                            }
                        }
                    }
                    string help = Help();
                    if (help != null)
                        await m_chan.SendMessageAsync(help);
                    m_time = DateTime.Now;
                }
            }
            public abstract string[] GetPost();
            public abstract bool IsPostImage();

            public async Task CheckCorrect(string userWord, IUser user)
            {
                if (m_time == DateTime.MinValue)
                {
                    await m_chan.SendMessageAsync(Sentences.WaitImage(m_guild.Id));
                    return;
                }
                bool sayCorrect;
                string msg = GetCheckCorrect(userWord, out sayCorrect);
                if (msg == null)
                {
                    m_time = DateTime.MinValue;
                    m_nbFound++;
                    if (!m_userIds.Contains(user.Id))
                        m_userIds.Add(user.Id);
                    if (sayCorrect)
                        await m_chan.SendMessageAsync(Sentences.GuessGood(m_guild.Id));
                    await Post();
                }
                else
                    await m_chan.SendMessageAsync(msg);
            }
            public abstract string GetCheckCorrect(string userWord, out bool sayCorrect);
            public abstract void Loose();
            public abstract string Help();

            public ITextChannel m_chan { private set; get; }
            protected IGuild m_guild { private set; get; }
            public bool m_didLost { protected set; get; }
            private readonly int m_refTime;
            public int GetRefTime() { return (m_refTime); }
            protected DateTime m_time { set; get; }

            protected int m_nbAttempt;
            protected int m_nbFound;
            protected List<ulong> m_userIds;
            protected string m_fileName;
        }

        public class Kancolle : Game
        {
            public Kancolle(IMessageChannel chan, IGuild guild, IUser charac, bool isEasy) : base(chan, guild, charac, kancolleTimer, "kancolle", isEasy, false)
            {
                m_shipNames = Program.p.kancolleDict;
                if (m_shipNames == null)
                    throw new NullReferenceException("Dictionary not available.");
                m_toGuess = null;
                m_idImage = "-1";
            }

            public override bool IsPostImage()
                => true;

            public override string Help()
                => null;

            public override string[] GetPost() // TODO: sometimes post wrong images
            {
                m_toGuess = m_shipNames[Program.p.rand.Next(m_shipNames.Count)];
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    string url = "https://kancolle.fandom.com/api/v1/Search/List?query=" + Uri.EscapeDataString(m_toGuess) + "&limit=1";
                    string json = w.DownloadString(url);
                    string code = Utilities.GetElementXml("\"id\":", json, ',');
                    m_idImage = code;
                    url = "https://kancolle.fandom.com/api/v1/Search/List?query=" + Uri.EscapeDataString(m_toGuess) + "/Gallery&limit=1";
                    json = w.DownloadString(url);
                    code = Utilities.GetElementXml("\"title\":\"", json, '"').Replace("\\", "");
                    string html;
                    using (HttpClient hc = new HttpClient())
                        html = hc.GetStringAsync("https://kancolle.fandom.com/wiki/" + code).GetAwaiter().GetResult();
                    return (new string[] { html.Split(new string[] { "img src=\"" }, StringSplitOptions.None)[2].Split('"')[0].Split(new string[] { "/revision" }, StringSplitOptions.None)[0] });
                }
            }

            public override string GetCheckCorrect(string userWord, out bool sayCorrect)
            {
                sayCorrect = true;
                m_nbAttempt++;
                try
                {
                    bool isSpace = true;
                    string newName = "";
                    foreach (char c in userWord)
                    {
                        if (c == ' ')
                        {
                            isSpace = true;
                            newName += ' ';
                        }
                        else
                        {
                            if (isSpace)
                                newName += char.ToUpper(c);
                            else
                                newName += c;
                            isSpace = false;
                        }
                    }
                    if (newName.ToUpper() == m_toGuess.ToUpper())
                        return (null);
                    using (WebClient w = new WebClient()) // TODO: Check if clean names match
                    {
                        string url = "https://kancolle.fandom.com/api/v1/Search/List?query=" + Uri.EscapeDataString(newName) + "&limit=1";
                        string json = w.DownloadString(url);
                        string code = Utilities.GetElementXml("\"title\":\"", json, '"');
                        url = "https://kancolle.fandom.com/wiki/" + code + "?action=raw";
                        url = url.Replace(' ', '_');
                        json = w.DownloadString(url);
                        if (Utilities.GetElementXml("{{", json, '}') != "ShipPageHeader")
                            return (Sentences.KancolleGuessDontExist(m_guild.Id));
                        w.Encoding = Encoding.UTF8;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        url = "https://kancolle.fandom.com/api/v1/Search/List?query=" + Uri.EscapeDataString(newName) + "&limit=1";
                        json = w.DownloadString(url);
                        code = Utilities.GetElementXml("\"id\":", json, ',');
                        if (m_idImage == code)
                            return (null);
                        return (Sentences.GuessBad(m_guild.Id, Utilities.GetElementXml("\"title\":", json, ',')));
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse code = ex.Response as HttpWebResponse;
                    if (code.StatusCode == HttpStatusCode.NotFound)
                        return (Sentences.KancolleGuessDontExist(m_guild.Id));
                    throw ex;
                }
            }

#pragma warning disable CS1998
            public override async void Loose()
            {
                SaveServerScores(m_toGuess);
            }
#pragma warning restore CS1998

            private string m_toGuess;
            private string m_idImage;
            private List<string> m_shipNames;
        }

        public class BooruGame : Game
        {
            public BooruGame(IMessageChannel chan, IGuild guild, IUser charac, bool isEasy) : base(chan, guild, charac, booruTimer, "booru", isEasy, false)
            {
                m_toGuess = null;
                m_allTags = Program.p.booruDict;
                if (m_allTags == null)
                    throw new NullReferenceException("Dictionary not available.");
                m_time = DateTime.MinValue;
                m_booru = new Gelbooru();
            }

            public override bool IsPostImage()
                => true;

            public override string Help()
            {
                string help = m_toGuess.First().ToString().ToUpper();
                foreach (char c in m_toGuess.Skip(1))
                {
                    if (c == '_')
                        help += ' ';
                    else
                        help += "\\*";
                }
                return help;
            }

            public override string[] GetPost()
            {
                m_toGuess = m_allTags[Program.p.rand.Next(m_allTags.Count)];
                return (new string[] {
                    Features.NSFW.Booru.SearchBooru(false, new string[] { m_toGuess }, m_booru, Program.p.rand).GetAwaiter().GetResult().answer.url,
                    Features.NSFW.Booru.SearchBooru(false, new string[] { m_toGuess }, m_booru, Program.p.rand).GetAwaiter().GetResult().answer.url,
                    Features.NSFW.Booru.SearchBooru(false, new string[] { m_toGuess }, m_booru, Program.p.rand).GetAwaiter().GetResult().answer.url,
                });
            }

            private bool IsClose(string str1, string str2)
            {
                string[] tmpStr1 = str1.Split(new string[] { " ", "_", "-" }, StringSplitOptions.RemoveEmptyEntries);
                string[] tmpStr2 = str2.Split(new string[] { " ", "_", "-" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s1 in tmpStr1)
                {
                    string newS1 = Utilities.CleanWord(s1);
                    foreach (string s2 in tmpStr2)
                    {
                        string newS2 = Utilities.CleanWord(s2);
                        if (newS1.Contains(newS2) || newS2.Contains(newS1))
                            return (true);
                    }
                }
                return (false);
            }

            public override string GetCheckCorrect(string userWord, out bool sayCorrect)
            {
                sayCorrect = true;
                m_nbAttempt++;
                if (Utilities.CleanWord(userWord) == Utilities.CleanWord(m_toGuess))
                    return (null);
                if (Utilities.CleanWord(userWord) != "" && IsClose(userWord, m_toGuess))
                    return (Sentences.BooruGuessClose(m_guild.Id, userWord));
                return (Sentences.GuessBad(m_guild.Id, userWord));
            }

#pragma warning disable CS1998
            public override async void Loose()
            {
                SaveServerScores(m_toGuess);
            }
#pragma warning restore CS1998
            
            private string m_toGuess;
            private List<string> m_allTags;
            private Gelbooru m_booru;
        }

        public class AnimeGame : Game
        {
            public AnimeGame(IMessageChannel chan, IGuild guild, IUser charac, bool isEasy, bool isFull) : base(chan, guild, charac, animeTimer, "anime", isEasy, isFull)
            {
                m_toGuess = null;
                m_allTags = isFull ? Program.p.animeFullDict : Program.p.animeDict;
                if (m_allTags == null)
                    throw new NullReferenceException("Dictionary not available.");
                m_time = DateTime.MinValue;
                m_booru = new Sakugabooru();
            }

            private string GetCorrectPost(out string tag, out List<string> allTags)
            {
                tag = m_allTags[Program.p.rand.Next(m_allTags.Count)];
                var result = Features.NSFW.Booru.SearchBooru(false, new string[] { tag, "animated" }, m_booru, Program.p.rand).GetAwaiter().GetResult();
                allTags = result.answer.tags.ToList();
                return (result.answer.url);
            }

            public override bool IsPostImage()
                => true;

            public override string Help()
            {
                string help = m_toGuess[0].First().ToString().ToUpper();
                foreach (char c in m_toGuess[0].Skip(1))
                {
                    if (c == '_')
                        help += ' ';
                    else
                        help += "\\*";
                }
                return help;
            }

            public override string[] GetPost()
            {
                string tag;
                List<string> allTags;
                string image = GetCorrectPost(out tag, out allTags);
                allTags.RemoveAll(x => m_booru.GetTag(x).GetAwaiter().GetResult().type != BooruSharp.Search.Tag.TagType.Copyright);
                m_toGuess = allTags.ToArray();
                return (new string[] { image });
            }

            public override string GetCheckCorrect(string userWord, out bool sayCorrect)
            {
                sayCorrect = true;
                m_nbAttempt++;
                foreach (string answer in m_toGuess)
                {
                    if (Utilities.CleanWord(userWord) == Utilities.CleanWord(answer))
                        return (null);
                    if (Utilities.CleanWord(userWord) != "" && (Utilities.CleanWord(answer).Contains(Utilities.CleanWord(userWord)) || Utilities.CleanWord(userWord).Contains(Utilities.CleanWord(answer))))
                        return (Sentences.BooruGuessClose(m_guild.Id, userWord));
                }
                return (Sentences.GuessBad(m_guild.Id, userWord));
            }

#pragma warning disable CS1998
            public override async void Loose()
            {
                SaveServerScores(m_toGuess[0]);
            }
#pragma warning restore CS1998

            private string[] m_toGuess;
            private List<string> m_allTags;
            private Sakugabooru m_booru;
        }

        public class AzurLane : Game
        {
            public AzurLane(IMessageChannel chan, IGuild guild, IUser charac, bool isEasy) : base(chan, guild, charac, azurlaneTimer, "azurlane", isEasy, false)
            {
                m_shipNames = Program.p.azurLaneDict;
                if (m_shipNames == null)
                    throw new NullReferenceException("Dictionary not available.");
                m_toGuess = null;
            }

            public override bool IsPostImage()
                => true;

            public override string Help()
                => null;

            public override string[] GetPost()
            {
                m_toGuess = m_shipNames[Program.p.rand.Next(m_shipNames.Count)];
                JArray json;
                using (HttpClient hc = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    json = JArray.Parse(hc.GetStringAsync("https://azurlane.koumakan.jp/w/api.php?action=opensearch&search=" + Uri.EscapeDataString(m_toGuess).Replace("%20", "+") + "&limit=1").GetAwaiter().GetResult());
                }
                string[] nameArray = json[1].ToObject<string[]>();
                using (HttpClient hc = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    try
                    {
                        return (new string[] { "https://azurlane.koumakan.jp" + Regex.Match(hc.GetStringAsync("https://azurlane.koumakan.jp/" + nameArray[0].Replace(" ", "_")).GetAwaiter().GetResult(),
                        "src=\"(\\/w\\/images\\/thumb\\/[^\\/]+\\/[^\\/]+\\/[^\\/]+\\/[0-9]+px-" + m_toGuess.Replace(" ", "_") + ".png)").Groups[1].Value });
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error, invalid search: " + Uri.EscapeDataString(m_toGuess.Replace(" ", "+")));
                        return (GetPost());
                    }
                }
            }

            public override string GetCheckCorrect(string userWord, out bool sayCorrect)
            {
                sayCorrect = true;
                m_nbAttempt++;
                if (Regex.Replace(userWord, "[^a-zA-Z0-9]", "").ToLower() == Regex.Replace(m_toGuess, "[^a-zA-Z0-9]", "").ToLower())
                    return (null);
                JArray json;
                using (HttpClient hc = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    json = JArray.Parse(hc.GetStringAsync("https://azurlane.koumakan.jp/w/api.php?action=opensearch&search=" + Uri.EscapeDataString(userWord.Replace(" ", "%20")) + "&limit=1").GetAwaiter().GetResult());
                }
                string[] nameArray = json[1].ToObject<string[]>();
                if (nameArray.Length == 0)
                    return (Sentences.GuessBad(m_guild.Id, userWord));
                if (nameArray[0] == m_toGuess)
                    return (Sentences.BooruGuessClose(m_guild.Id, userWord));
                return (Sentences.GuessBad(m_guild.Id, nameArray[0]));
            }

#pragma warning disable CS1998
            public override async void Loose()
            {
                SaveServerScores(m_toGuess);
            }
#pragma warning restore CS1998

            private string m_toGuess;
            private List<string> m_shipNames;
        }
    }
}