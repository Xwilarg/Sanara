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
using System.Threading;
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
            protected Game(IMessageChannel chan, IGuild guild, IUser charac, int refTime)
            {
                m_chan = chan;
                m_score = 0;
                m_didLost = false;
                m_refTime = refTime;
                m_time = DateTime.Now;
                m_charac = Program.p.relations.Find(x => x._name == charac.Id);
                m_guild = guild;
            }

            public bool IsGameLost()
            {
                return (m_didLost || m_time != DateTime.MinValue && m_time.AddSeconds(m_refTime).CompareTo(DateTime.Now) == -1);
            }

            protected async void SaveServerScores(int nbFound, int nbAttempt, string fileName, List<ulong> usersId, string answer)
            {
                string[] datas;
                if (File.Exists("Saves/Servers/" + m_guild.Id + "/" + fileName))
                    datas = File.ReadAllLines("Saves/Servers/" + m_guild.Id + "/" + fileName);
                else
                    datas = new string[] { "0", "0", "0", "0", "0" };
                string allUsers = "";
                if (nbFound > Convert.ToInt32(datas[3]))
                {
                    foreach (ulong u in usersId)
                    {
                        allUsers += u.ToString() + "|";
                    }
                    if (allUsers != "")
                        allUsers = allUsers.Substring(0, allUsers.Length - 1);
                }
                else
                {
                    allUsers = datas[4];
                }
                File.WriteAllText("Saves/Servers/" + m_guild.Id + "/" + fileName,
                    (Convert.ToInt32(datas[0]) + 1).ToString() + Environment.NewLine +
                    (Convert.ToInt32(datas[1]) + nbAttempt).ToString() + Environment.NewLine +
                    (Convert.ToInt32(datas[2]) + nbFound).ToString() + Environment.NewLine +
                    ((nbFound > Convert.ToInt32(datas[3])) ? (nbFound.ToString()) : (Convert.ToInt32(datas[3]).ToString())) + Environment.NewLine +
                    allUsers);
                string finalStr = "Time out, the answer was " + answer + "." + Environment.NewLine;
                if (nbFound > Convert.ToInt32(datas[3]))
                    finalStr += "Congratulation, you beat the previous best score of " + Convert.ToInt32(datas[3]) + " with a new score of " + nbFound + ".";
                else if (nbFound == Convert.ToInt32(datas[3]))
                    finalStr += "You equilized the previous best score of " + nbFound + ".";
                else
                    finalStr += "You didn't beat the current best score of " + Convert.ToInt32(datas[3]) + " with the score of " + nbFound + ".";
                await m_chan.SendMessageAsync(finalStr);
            }

            public abstract void Post();
            public abstract void CheckCorrect(string userWord, IUser user);
            public abstract void Loose();

            public IMessageChannel m_chan { private set; get; }
            protected IGuild m_guild { private set; get; }
            public int m_score { protected set; get; }
            public bool m_didLost { protected set; get; }
            private int m_refTime;
            protected DateTime m_time { set; get; }
            protected Character m_charac { private set; get; }
        }

        public class Shiritori : Game
        {
            public Shiritori(IMessageChannel chan, IGuild guild, IUser charac) : base(chan, guild, charac, shiritoriTimer)
            {
                m_currWord = null;
                m_words = File.ReadAllLines("Saves/shiritoriWords.dat").ToList();
                m_alreadySaid = new List<string>();
            }

            public override async void Post()
            {
                if (m_currWord == null)
                {
                    m_currWord = "しりとり";
                    await m_chan.SendMessageAsync("しりとり (shiritori)");
                }
                else
                {
                    string[] corrWords = m_words.Where(x => x[0] == m_currWord[m_currWord.Length - 1]).ToArray();
                    if (corrWords.Length == 0)
                    {
                        await m_chan.SendMessageAsync("I don't know any other word...");
                    }
                    else
                    {
                        string word = corrWords[Program.p.rand.Next(0, corrWords.Length)];
                        string[] insideWord = word.Split('$');
                        await m_chan.SendMessageAsync(insideWord[0] + " (" + LinguistModule.fromHiragana(insideWord[0]) + ") - Meaning: " + insideWord[1]);
                        m_words.Remove(insideWord[0]);
                        m_currWord = insideWord[0];
                        m_alreadySaid.Add(insideWord[0]);
                    }
                }
                m_time = DateTime.Now;
            }

            //TODO Manage kanjis using Jisho API
            public override async void CheckCorrect(string userWord, IUser user)
            {
                if (user.Id != m_charac._name)
                {
                    await m_chan.SendMessageAsync("I'm sorry but you're not concerned by this game.");
                }
                else if (m_time == DateTime.MinValue)
                {
                    await m_chan.SendMessageAsync("Please wait until I'm playing.");
                }
                else
                {
                    userWord = LinguistModule.fromKatakana(LinguistModule.toHiragana(userWord));
                    foreach (char c in userWord)
                    {
                        if (c < 0x0031 || (c > 0x005A && c < 0x0061) || (c > 0x007A && c < 0x3041) || (c > 0x3096 && c < 0x30A1) || c > 0x30FA)
                        {
                            await m_chan.SendMessageAsync("Please only use hiragana, katakana or romaji.");
                            return;
                        }
                    }
                    string json;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + userWord);
                    }
                    bool isCorrect = false;
                    foreach (string s in Program.getElementXml("\"japanese\":[", json, '$').Split(new string[] { "\"japanese\":[" }, StringSplitOptions.None))
                    {
                        string hiragana = Program.getElementXml("\"reading\":\"", s, '"');
                        if (userWord == hiragana)
                        {
                            isCorrect = true;
                            if (Program.getElementXml("parts_of_speech\":[\"", json, '"') != "Noun")
                            {
                                await m_chan.SendMessageAsync("This word isn't a noun.");
                                return;
                            }
                            break;
                        }
                    }
                    if (!isCorrect)
                    {
                        await m_chan.SendMessageAsync("This word doesn't exist.");
                        return;
                    }
                    if (userWord[0] != m_currWord[m_currWord.Length - 1])
                    {
                        await m_chan.SendMessageAsync("Your word must begin by a " + m_currWord[m_currWord.Length - 1] + " (" + LinguistModule.fromHiragana(m_currWord[m_currWord.Length - 1].ToString()) + ").");
                        return;
                    }
                    if (m_alreadySaid.Contains(userWord))
                    {
                        await m_chan.SendMessageAsync("This word was already said.");
                        m_didLost = true;
                        return;
                    }
                    if (userWord[userWord.Length - 1] == 'ん')
                    {
                        await m_chan.SendMessageAsync("Your word is finishing with a ん.");
                        m_didLost = true;
                        return;
                    }
                    m_time = DateTime.MinValue;
                    m_words.Remove(userWord);
                    m_currWord = userWord;
                    Post();
                    m_score++;
                }
            }

            public override async void Loose() // TODO: Save score
            {
                string finalStr = "You lost." + Environment.NewLine;
                string[] corrWords = m_words.Where(x => x[0] == m_currWord[m_currWord.Length - 1]).ToArray();
                if (corrWords.Length == 0)
                {
                    finalStr += "To be honest, I didn't know a word to answer too." + Environment.NewLine;
                }
                else
                {
                    string word = corrWords[Program.p.rand.Next(0, corrWords.Length)];
                    string[] insideWord = word.Split('$');
                    finalStr += "Here's a word you could have said: " + insideWord[0] + " (" + LinguistModule.fromHiragana(insideWord[0]) + ") - Meaning: " + insideWord[1] + Environment.NewLine;
                }
                finalStr += "You did a score of " + m_score + Environment.NewLine;
                await m_chan.SendMessageAsync(finalStr);
            }

            private string m_currWord;
            private List<string> m_words;
            private List<string> m_alreadySaid;

        }

        public class Kancolle : Game
        {
            public Kancolle(IMessageChannel chan, IGuild guild, IUser charac) : base(chan, guild, charac, kancolleTimer)
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
                m_shipAttempt = 0;
                m_shipFound = 0;
                m_userIds = new List<ulong>();
                m_idImage = "-1";
            }

            public override async void Post() // TODO: sometimes post wrong images
            {
                m_toGuess = m_shipNames[Program.p.rand.Next(m_shipNames.Count)];
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    string url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + m_toGuess + "&limit=1";
                    string json = w.DownloadString(url);
                    string code = Program.getElementXml("\"id\":", json, ',');
                    m_idImage = code;
                    url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + m_toGuess + "/Gallery&limit=1";
                    json = w.DownloadString(url);
                    code = Program.getElementXml("\"id\":", json, ',');
                    url = "http://kancolle.wikia.com/api/v1/Articles/Details?ids=" + code;
                    json = w.DownloadString(url);
                    string image = Program.getElementXml("\"thumbnail\":\"", json, '"');
                    image = image.Split(new string[] { ".jpg" }, StringSplitOptions.None)[0] + ".jpg";
                    image = image.Replace("\\", "");
                    int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                    w.DownloadFile(image, "shipgirlquizz" + currentTime + ".jpg");
                    await m_chan.SendFileAsync("shipgirlquizz" + currentTime + ".jpg");
                    File.Delete("shipgirlquizz" + currentTime + ".jpg");
                    m_time = DateTime.Now;
                }
            }
            
            public override async void CheckCorrect(string userWord, IUser user)
            {
                if (m_time == DateTime.MinValue)
                {
                    await m_chan.SendMessageAsync("Please wait until I posted the image.");
                }
                else
                {
                    m_shipAttempt++;
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
                            string code = Program.getElementXml("\"title\":\"", json, '"');
                            url = "http://kancolle.wikia.com/wiki/" + code + "?action=raw";
                            url = url.Replace(' ', '_');
                            json = w.DownloadString(url);
                            if (Program.getElementXml("{{", json, '}') != "ShipPageHeader")
                            {
                                await m_chan.SendMessageAsync("There is no shipgirl with this name...");
                            }
                            else
                            {
                                w.Encoding = Encoding.UTF8;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + newName + "&limit=1";
                                json = w.DownloadString(url);
                                code = Program.getElementXml("\"id\":", json, ',');
                                if (m_idImage == code)
                                {
                                    m_time = DateTime.MinValue;
                                    if (!m_userIds.Contains(user.Id))
                                        m_userIds.Add(user.Id);
                                    await m_chan.SendMessageAsync("Congratulation, you found the right answer!");
                                    Post();
                                    m_shipFound++;
                                }
                                else
                                {
                                    await m_chan.SendMessageAsync("No, this is not " + newName + ".");
                                }
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        HttpWebResponse code = ex.Response as HttpWebResponse;
                        if (code.StatusCode == HttpStatusCode.NotFound)
                            await m_chan.SendMessageAsync("There is no shipgirl with this name...");
                    }
                }
            }

#pragma warning disable CS1998
            public override async void Loose()
            {
                SaveServerScores(m_shipFound, m_shipAttempt, "kancolle.dat", m_userIds, m_toGuess);
            }
#pragma warning restore CS1998

            private string m_toGuess;
            private int m_shipAttempt;
            private int m_shipFound;
            private List<ulong> m_userIds;
            private string m_idImage;
            private List<string> m_shipNames;
        }

        public class BooruGame : Game
        {
            public BooruGame(IMessageChannel chan, IGuild guild, IUser charac) : base(chan, guild, charac, booruTimer)
            {
                m_booruAttempt = 0;
                m_booruFound = 0;
                m_userIds = new List<ulong>();
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

#pragma warning disable CS1998
            public override async void Post()
            {
                m_time = DateTime.MinValue;
                m_toGuess = m_allTags[Program.p.rand.Next(m_allTags.Count)];
                string currName = "booruGame" + DateTime.Now.ToString("HHmmssfff") + m_guild.Id.ToString();
                BooruModule.getImage(new BooruModule.Gelbooru(), new string[] { m_toGuess }, m_chan as ITextChannel, currName + "1", false, true);
                BooruModule.getImage(new BooruModule.Gelbooru(), new string[] { m_toGuess }, m_chan as ITextChannel, currName + "2", false, true);
                BooruModule.getImage(new BooruModule.Gelbooru(), new string[] { m_toGuess }, m_chan as ITextChannel, currName + "3", false, true);
                m_time = DateTime.Now;
            }
#pragma warning restore CS1998

            public override async void CheckCorrect(string userWord, IUser user)
            {
                if (m_time == DateTime.MinValue)
                    await m_chan.SendMessageAsync("Please wait until I posted all images.");
                else
                {
                    m_booruAttempt++;
                    if (Program.cleanWord(userWord) == Program.cleanWord(m_toGuess))
                    {
                        m_booruFound++;
                        if (!m_userIds.Contains(user.Id))
                            m_userIds.Add(user.Id);
                        await m_chan.SendMessageAsync("You found the right answer.");
                        Post();
                    }
                    else if (Program.cleanWord(m_toGuess).Contains(Program.cleanWord(userWord)) || Program.cleanWord(userWord).Contains(Program.cleanWord(m_toGuess)))
                        await m_chan.SendMessageAsync("No, this is not " + userWord + " but you're close to the answer.");
                    else
                        await m_chan.SendMessageAsync("No, this is not " + userWord + ".");
                }
            }

#pragma warning disable CS1998
            public override async void Loose()
            {
                SaveServerScores(m_booruFound, m_booruAttempt, "booru.dat", m_userIds, m_toGuess);
            }
#pragma warning restore CS1998

            private int m_booruAttempt;
            private int m_booruFound;
            private List<ulong> m_userIds;
            private string m_toGuess;
            private List<string> m_allTags;
        }

        [Command("Play"), Summary("Launch a game")]
        public async Task playShiritori(params string[] gameName)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (p.games.Any(x => x.m_chan == Context.Channel))
            {
                await ReplyAsync(Sentences.gameAlreadyrunning);
            }
            else
            {
                string finalGameName = Program.addArgs(gameName);
                if (finalGameName == null || (finalGameName.ToLower() != "shiritori" && finalGameName.ToLower() != "kancolle" && finalGameName.ToLower() != "booru"))
                {
                    await ReplyAsync(Sentences.invalidGameName);
                }
                else
                {
                    if (!p.gameThread.IsAlive)
                        p.gameThread.Start();
                    Game g = null;
                    if (finalGameName.ToLower() == "shiritori")
                    {
                        await ReplyAsync(Sentences.rulesShiritori);
                        g = new Shiritori(Context.Channel, Context.Guild, Context.User);
                    }
                    else if (finalGameName.ToLower() == "kancolle")
                    {
                        await ReplyAsync(Sentences.rulesKancolle);
                        g = new Kancolle(Context.Channel, Context.Guild, Context.User);
                    }
                    else if (finalGameName.ToLower() == "booru")
                    {
                        if (!(Context.Channel as ITextChannel).IsNsfw)
                        {
                            await ReplyAsync(Sentences.chanIsNotNsfw);
                            return;
                        }
                        await ReplyAsync(Sentences.rulesBooru);
                        g = new BooruGame(Context.Channel, Context.Guild, Context.User);
                    }
                    p.games.Add(g);
                    g.Post();
                }
            }
        }
    }

    partial class Program
    {
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
                } catch (InvalidOperationException)
                { }
                Thread.Sleep(100);
            }
        }
    }
}