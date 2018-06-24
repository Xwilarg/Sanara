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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class GameModule : ModuleBase
    {
        Program p = Program.p;

        public static readonly int shiritoriTimer = 10;
        public static readonly int kancolleTimer = 10;
        public static readonly int booruTimer = 45;

        public abstract class Game
        {
            protected Game(IMessageChannel chan, IGuild guild, IUser charac, int refTime, string fileName, bool isEasy)
            {
                m_chan = chan;
                m_didLost = false;
                m_refTime = refTime * ((isEasy) ? (2) : (1));
                m_time = DateTime.Now;
                m_charac = Program.p.relations.Find(x => x._name == charac.Id);
                m_guild = guild;
                m_nbAttempt = 0;
                m_nbFound = 0;
                m_userIds = new List<ulong>();
                m_fileName = fileName + ((isEasy) ? ("-easy") : (""));
            }

            public bool IsGameLost()
            {
                return (m_didLost || (m_time != DateTime.MinValue && m_time.AddSeconds(m_refTime).CompareTo(DateTime.Now) == -1));
            }

            protected async void SaveServerScores(string answer)
            {
                string[] datas;
                if (File.Exists("Saves/Servers/" + m_guild.Id + "/" + m_fileName))
                    datas = File.ReadAllLines("Saves/Servers/" + m_guild.Id + "/" + m_fileName);
                else
                    datas = new string[] { "0", "0", "0", "0", "0" };
                string allUsers = "";
                if (m_nbFound > Convert.ToInt32(datas[3]))
                    allUsers = String.Join("|", m_userIds.Select(x => x.ToString()));
                else
                    allUsers = datas[4];
                File.WriteAllText("Saves/Servers/" + m_guild.Id + "/" + m_fileName,
                    (Convert.ToInt32(datas[0]) + 1).ToString() + Environment.NewLine +
                    (Convert.ToInt32(datas[1]) + m_nbAttempt).ToString() + Environment.NewLine +
                    (Convert.ToInt32(datas[2]) + m_nbFound).ToString() + Environment.NewLine +
                    ((m_nbFound > Convert.ToInt32(datas[3])) ? (m_nbFound.ToString()) : (Convert.ToInt32(datas[3]).ToString())) + Environment.NewLine +
                    allUsers);
                string finalStr = (answer == null) ? ("") : (Sentences.TimeoutGame(m_guild.Id, answer) + Environment.NewLine);
                if (m_nbFound > Convert.ToInt32(datas[3]))
                    finalStr += Sentences.NewBestScore(m_guild.Id, Convert.ToInt32(datas[3]).ToString(), m_nbFound.ToString());
                else if (m_nbFound == Convert.ToInt32(datas[3]))
                    finalStr += Sentences.EqualizedScore(m_guild.Id, m_nbFound.ToString());
                else
                    finalStr += Sentences.DidntBeatScore(m_guild.Id, Convert.ToInt32(datas[3]).ToString(), m_nbFound.ToString());
                await m_chan.SendMessageAsync(finalStr);
            }

            public void Post()
            {
                _ = Task.Run(async delegate() {
                    bool isMsg;
                    foreach (string msg in GetPost(out isMsg))
                    {
                        if (isMsg)
                            await m_chan.SendMessageAsync(msg);
                        else
                        {
                            await m_chan.SendFileAsync(msg);
                            File.Delete(msg);
                        }
                    }
                });
            }
            public abstract string[] GetPost(out bool isMsg);
            public abstract void CheckCorrect(string userWord, IUser user);
            public abstract void Loose();

            public IMessageChannel m_chan { private set; get; }
            protected IGuild m_guild { private set; get; }
            public bool m_didLost { protected set; get; }
            private readonly int m_refTime;
            protected DateTime m_time { set; get; }
            protected Character m_charac { private set; get; }

            protected int m_nbAttempt;
            protected int m_nbFound;
            protected List<ulong> m_userIds;
            protected string m_fileName;
        }

        public class Shiritori : Game
        {
            public Shiritori(IMessageChannel chan, IGuild guild, IUser charac, bool isEasy) : base(chan, guild, charac, shiritoriTimer, "shiritori.dat", isEasy)
            {
                m_currWord = null;
                m_words = File.ReadAllLines("Saves/shiritoriWords.dat").ToList();
                m_alreadySaid = new List<string>();
            }

            public override string[] GetPost(out bool isMsg)
            {
                isMsg = true;
                if (m_currWord == null)
                {
                    m_currWord = "しりとり";
                    m_words.Remove(m_words.Find(x => x.Split('$')[0] == m_currWord));
                    m_alreadySaid.Add(m_currWord);
                    return (new string[] { "しりとり (shiritori)" });
                }
                else
                {
                    string[] corrWords = m_words.Where(x => x[0] == m_currWord[m_currWord.Length - 1]).ToArray();
                    if (corrWords.Length == 0)
                        return (new string[] { Sentences.ShiritoriNoWord(m_guild.Id) }); // TODO what happen then ?
                    else
                    {
                        string word = corrWords[Program.p.rand.Next(0, corrWords.Length)];
                        string[] insideWord = word.Split('$');
                        m_words.Remove(word);
                        m_alreadySaid.Add(insideWord[0]);
                        m_currWord = insideWord[0];
                        m_time = DateTime.Now;
                        return (new string[] { insideWord[0] + " (" + LinguistModule.FromHiragana(LinguistModule.FromKatakana(insideWord[0])) + ") - Meaning: " + insideWord[1] });
                    }
                }
            }

            //TODO Manage kanjis using Jisho API
            public override async void CheckCorrect(string userWord, IUser user)
            {
                if (m_time == DateTime.MinValue)
                {
                    await m_chan.SendMessageAsync(Sentences.WaitPlay(m_guild.Id));
                }
                else
                {
                    DateTime now = DateTime.Now;
                    m_time = DateTime.MinValue;
                    m_nbAttempt++;
                    userWord = LinguistModule.ToHiragana(LinguistModule.FromKatakana(userWord));
                    if (userWord.Any(c => c < 0x0031 || (c > 0x005A && c < 0x0061) || (c > 0x007A && c < 0x3041) || (c > 0x3096 && c < 0x30A1) || c > 0x30FA))
                    {
                        m_time = now;
                        await m_chan.SendMessageAsync(Sentences.OnlyHiraganaKatakanaRomaji(m_guild.Id));
                        return;
                    }
                    string json;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + userWord);
                    }
                    bool isCorrect = false;
                    bool isNoun = false;
                    foreach (string s in Utilities.GetElementXml("\"japanese\":[", json, '$').Split(new string[] { "\"japanese\":[" }, StringSplitOptions.None))
                    {
                        string hiragana = LinguistModule.ToHiragana(LinguistModule.FromKatakana(Utilities.GetElementXml("\"reading\":\"", s, '"')));
                        if (userWord == hiragana)
                        {
                            isCorrect = true;
                            foreach (string pos in Utilities.GetElementXml("parts_of_speech\":[", json, ']').Split(','))
                            {
                                if (pos == "\"Noun\"")
                                {
                                    isNoun = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!isCorrect)
                    {
                        await m_chan.SendMessageAsync(Sentences.ShiritoriDoesntExist(m_guild.Id));
                        m_time = now;
                        return;
                    }
                    if (!isNoun)
                    {
                        await m_chan.SendMessageAsync(Sentences.ShiritoriNotNoun(m_guild.Id));
                        m_time = now;
                        return;
                    }
                    if (userWord[0] != HiraganaToUpper(m_currWord[m_currWord.Length - 1]))
                    {
                        await m_chan.SendMessageAsync(Sentences.ShiritoriMustBegin(m_guild.Id, HiraganaToUpper(m_currWord[m_currWord.Length - 1]).ToString(), LinguistModule.FromHiragana(m_currWord[m_currWord.Length - 1].ToString())));
                        m_time = now;
                        return;
                    }
                    if (m_alreadySaid.Contains(userWord))
                    {
                        await m_chan.SendMessageAsync(Sentences.ShiritoriAlreadySaid(m_guild.Id));
                        m_didLost = true;
                        return;
                    }
                    if (userWord[userWord.Length - 1] == 'ん')
                    {
                        await m_chan.SendMessageAsync(Sentences.ShiritoriEndWithN(m_guild.Id));
                        m_didLost = true;
                        return;
                    }
                    m_nbFound++;
                    if (!m_userIds.Contains(user.Id))
                        m_userIds.Add(user.Id);
                    m_words.Remove(m_words.Find(x => x.Split('$')[0] == userWord));
                    m_alreadySaid.Add(userWord);
                    m_currWord = userWord;
                    Post();
                }
            }

            private char HiraganaToUpper(char current)
            {
                if (current == 'ゃ') return ('や');
                if (current == 'ぃ') return ('い');
                if (current == 'ゅ') return ('ゆ');
                if (current == 'ぇ') return ('え');
                if (current == 'ょ') return ('よ');
                return (current);
            }

            public override async void Loose()
            {
                string finalStr = Sentences.LostStr(m_guild.Id) + Environment.NewLine;
                string[] corrWords = m_words.Where(x => x[0] == HiraganaToUpper(m_currWord[m_currWord.Length - 1])).ToArray();
                if (corrWords.Length == 0)
                    finalStr += Sentences.ShiritoriNoMoreWord(m_guild.Id) + Environment.NewLine;
                else
                {
                    string word = corrWords[Program.p.rand.Next(0, corrWords.Length)];
                    string[] insideWord = word.Split('$');
                    finalStr += Sentences.ShiritoriSuggestion(m_guild.Id, insideWord[0], LinguistModule.FromHiragana(insideWord[0]), insideWord[1]) + Environment.NewLine;
                }
                await m_chan.SendMessageAsync(finalStr);
                SaveServerScores(null);
            }

            private string m_currWord;
            private List<string> m_words;
            private List<string> m_alreadySaid;
        }

        public class Kancolle : Game
        {
            public Kancolle(IMessageChannel chan, IGuild guild, IUser charac, bool isEasy) : base(chan, guild, charac, kancolleTimer, "kancolle.dat", isEasy)
            {
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    string json = w.DownloadString("http://kancolle.wikia.com/wiki/Ship?action=raw");
                    string[] cathegories = json.Split(new string[] { "==" }, StringSplitOptions.None);
                    bool didBegan = false;
                    string shipName = "";
                    bool beginRead = false;
                    bool readBeginLine = true;
                    m_shipNames = new List<string>();
                    foreach (char c in cathegories[2]) // Get all ship's name
                    {
                        if (!didBegan && c == '<')
                        {
                            didBegan = true;
                        }
                        else if (didBegan)
                        {
                            if (c == '[' && readBeginLine)
                            {
                                beginRead = true;
                                shipName = "";
                            }
                            else if ((c == '|' || c == ']') && shipName != "" && beginRead)
                            {
                                m_shipNames.Add(shipName);
                                shipName = "";
                                beginRead = false;
                            }
                            else if (c == '\n')
                                readBeginLine = true;
                            else
                            {
                                shipName += c;
                                readBeginLine = false;
                            }
                        }
                    }
                }
                m_toGuess = null;
                m_idImage = "-1";
            }

            public override string[] GetPost(out bool isMsg) // TODO: sometimes post wrong images
            {
                isMsg = false;
                m_toGuess = m_shipNames[Program.p.rand.Next(m_shipNames.Count)];
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    string url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + m_toGuess + "&limit=1";
                    string json = w.DownloadString(url);
                    string code = Utilities.GetElementXml("\"id\":", json, ',');
                    m_idImage = code;
                    url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + m_toGuess + "/Gallery&limit=1";
                    json = w.DownloadString(url);
                    code = Utilities.GetElementXml("\"id\":", json, ',');
                    url = "http://kancolle.wikia.com/api/v1/Articles/Details?ids=" + code;
                    json = w.DownloadString(url);
                    string image = Utilities.GetElementXml("\"thumbnail\":\"", json, '"');
                    image = image.Split(new string[] { ".jpg" }, StringSplitOptions.None)[0] + ".jpg";
                    image = image.Replace("\\", "");
                    int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                    w.DownloadFile(image, "shipgirlquizz" + currentTime + ".jpg");
                    m_time = DateTime.Now;
                    return (new string[] { "shipgirlquizz" + currentTime + ".jpg" });
                }
            }
            
            public override async void CheckCorrect(string userWord, IUser user)
            {
                if (m_time == DateTime.MinValue)
                {
                    await m_chan.SendMessageAsync(Sentences.WaitImage(m_guild.Id));
                }
                else
                {
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
                        using (WebClient w = new WebClient())
                        {
                            string url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + newName + "&limit=1";
                            string json = w.DownloadString(url);
                            string code = Utilities.GetElementXml("\"title\":\"", json, '"');
                            url = "http://kancolle.wikia.com/wiki/" + code + "?action=raw";
                            url = url.Replace(' ', '_');
                            json = w.DownloadString(url);
                            if (Utilities.GetElementXml("{{", json, '}') != "ShipPageHeader")
                            {
                                await m_chan.SendMessageAsync(Sentences.KancolleGuessDontExist(m_guild.Id));
                            }
                            else
                            {
                                w.Encoding = Encoding.UTF8;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + newName + "&limit=1";
                                json = w.DownloadString(url);
                                code = Utilities.GetElementXml("\"id\":", json, ',');
                                if (m_idImage == code)
                                {
                                    m_time = DateTime.MinValue;
                                    m_nbFound++;
                                    if (!m_userIds.Contains(user.Id))
                                        m_userIds.Add(user.Id);
                                    await m_chan.SendMessageAsync(Sentences.GuessGood(m_guild.Id));
                                    Post();
                                }
                                else
                                {
                                    await m_chan.SendMessageAsync(Sentences.BooruGuessBad(m_guild.Id, newName));
                                }
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        HttpWebResponse code = ex.Response as HttpWebResponse;
                        if (code.StatusCode == HttpStatusCode.NotFound)
                            await m_chan.SendMessageAsync(Sentences.KancolleGuessDontExist(m_guild.Id));
                    }
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
            public BooruGame(IMessageChannel chan, IGuild guild, IUser charac, bool isEasy) : base(chan, guild, charac, booruTimer, "booru.dat", isEasy)
            {
                m_toGuess = null;
                m_allTags = new List<string>();
                string[] allLines = File.ReadAllLines("Saves/BooruTriviaTags.dat");
                foreach (string line in allLines)
                {
                    string[] linePart = line.Split(' ');
                    if (Convert.ToInt32(linePart[1]) > 3)
                        m_allTags.Add(linePart[0]);
                }
                m_time = DateTime.MinValue;
            }

            public override string[] GetPost(out bool isMsg)
            {
                isMsg = false;
                m_time = DateTime.MinValue;
                m_toGuess = m_allTags[Program.p.rand.Next(m_allTags.Count)];
                string currName = "booruGame" + DateTime.Now.ToString("HHmmssfff") + m_guild.Id.ToString();
                m_time = DateTime.Now;
                return (new string[] {
                    BooruModule.GetImage(new BooruModule.Gelbooru(), new string[] { m_toGuess }),
                    BooruModule.GetImage(new BooruModule.Gelbooru(), new string[] { m_toGuess }),
                    BooruModule.GetImage(new BooruModule.Gelbooru(), new string[] { m_toGuess })
                });
            }

            public override async void CheckCorrect(string userWord, IUser user)
            {
                if (m_time == DateTime.MinValue)
                    await m_chan.SendMessageAsync(Sentences.WaitImages(m_guild.Id));
                else
                {
                    m_nbAttempt++;
                    if (Utilities.CleanWord(userWord) == Utilities.CleanWord(m_toGuess))
                    {
                        m_nbFound++;
                        if (!m_userIds.Contains(user.Id))
                            m_userIds.Add(user.Id);
                        await m_chan.SendMessageAsync(Sentences.GuessGood(m_guild.Id));
                        Post();
                    }
                    else if (Utilities.CleanWord(userWord) != "" && (Utilities.CleanWord(m_toGuess).Contains(Utilities.CleanWord(userWord)) || Utilities.CleanWord(userWord).Contains(Utilities.CleanWord(m_toGuess))))
                        await m_chan.SendMessageAsync(Sentences.BooruGuessClose(m_guild.Id, userWord));
                    else
                        await m_chan.SendMessageAsync(Sentences.BooruGuessBad(m_guild.Id, userWord));
                }
            }

#pragma warning disable CS1998
            public override async void Loose()
            {
                SaveServerScores(m_toGuess);
            }
#pragma warning restore CS1998
            
            private string m_toGuess;
            private List<string> m_allTags;
        }

        [Command("Play"), Summary("Launch a game")]
        public async Task PlayShiritori(params string[] gameName)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (p.games.Any(x => x.m_chan == Context.Channel))
                await ReplyAsync(Sentences.GameAlreadyRunning(Context.Guild.Id));
            else if (gameName.Length == 0)
                await ReplyAsync(Sentences.InvalidGameName(Context.Guild.Id));
            else
            {
                if (gameName[0].ToLower() != "shiritori" && gameName[0].ToLower() != "kancolle" && gameName[0].ToLower() != "booru")
                {
                    await ReplyAsync(Sentences.InvalidGameName(Context.Guild.Id));
                }
                else if (gameName.Length > 1 && gameName[1].ToLower() != "normal" && gameName[1].ToLower() != "easy")
                {
                    await ReplyAsync(Sentences.InvalidDifficulty(Context.Guild.Id));
                }
                else
                {
                    if (!p.gameThread.IsAlive)
                        p.gameThread.Start();
                    Game g = null;
                    bool isEasy = (gameName.Length > 1 && gameName[1].ToLower() == "easy");
                    if (gameName[0].ToLower() == "shiritori")
                    {
                        await ReplyAsync(Sentences.RulesShiritori(Context.Guild.Id));
                        g = new Shiritori(Context.Channel, Context.Guild, Context.User, isEasy);
                    }
                    else if (gameName[0].ToLower() == "kancolle")
                    {
                        await ReplyAsync(Sentences.RulesKancolle(Context.Guild.Id));
                        g = new Kancolle(Context.Channel, Context.Guild, Context.User, isEasy);
                    }
                    else if (gameName[0].ToLower() == "booru")
                    {
                        if (!(Context.Channel as ITextChannel).IsNsfw)
                        {
                            await ReplyAsync(Sentences.ChanIsNotNsfw(Context.Guild.Id));
                            return;
                        }
                        await ReplyAsync(Sentences.RulesBooru(Context.Guild.Id));
                        g = new BooruGame(Context.Channel, Context.Guild, Context.User, isEasy);
                    }
                    p.games.Add(g);
                    g.Post();
                }
            }
        }
    }
}