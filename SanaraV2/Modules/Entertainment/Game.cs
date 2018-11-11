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
using SanaraV2.Modules.Base;
using SanaraV2.Modules.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Entertainment
{
    public class GameModule : ModuleBase
    {
        Program p = Program.p;

        public static readonly int shiritoriTimer = 10;
        public static readonly int kancolleTimer = 10;
        public static readonly int booruTimer = 30;
        public static readonly int animeTimer = 30;

        public abstract class Game
        {
            protected Game(IMessageChannel chan, IGuild guild, IUser charac, int refTime, string fileName, bool isEasy)
            {
                m_chan = chan;
                m_didLost = false;
                m_refTime = refTime * ((isEasy) ? (2) : (1));
                m_time = DateTime.Now;
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

            public async Task Post()
            {
                m_time = DateTime.MinValue;
                foreach (string msg in GetPost())
                {
                    if (Features.Utilities.IsImage(msg.Split('.').Last()))
                        await m_chan.SendMessageAsync("", false, new EmbedBuilder()
                        {
                            Color = Color.Blue,
                            ImageUrl = msg
                        }.Build());
                    else
                        await m_chan.SendMessageAsync(msg);
                }
                m_time = DateTime.Now;
            }
            public abstract string[] GetPost();
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

            public override string[] GetPost()
            {
                if (m_currWord == null)
                {
                    m_currWord = "しりとり";
                    m_words.Remove(m_words.Find(x => x.Split('$')[0] == m_currWord));
                    m_alreadySaid.Add(m_currWord);
                    return (new string[] { "しりとり (shiritori)" });
                }
                string[] corrWords = m_words.Where(x => x[0] == m_currWord[m_currWord.Length - 1]).ToArray();
                if (corrWords.Length == 0)
                    return (new string[] { Sentences.ShiritoriNoWord(m_guild.Id) }); // TODO what happen then
                string word = corrWords[Program.p.rand.Next(0, corrWords.Length)];
                string[] insideWord = word.Split('$');
                m_words.Remove(word);
                m_alreadySaid.Add(insideWord[0]);
                m_currWord = insideWord[0];
                return (new string[] { insideWord[0] + " (" + Features.Tools.Linguist.ToRomaji(insideWord[0]) + ") - Meaning: " + insideWord[1] });
            }

            //TODO Manage kanjis using Jisho API
            public override string GetCheckCorrect(string userWord, out bool sayCorrect)
            {
                sayCorrect = false;
                m_time = DateTime.MinValue;
                m_nbAttempt++;
                userWord = Features.Tools.Linguist.ToHiragana(userWord);
                if (userWord.Any(c => c < 0x0041 || (c > 0x005A && c < 0x0061) || (c > 0x007A && c < 0x3041) || (c > 0x3096 && c < 0x30A1) || c > 0x30FA))
                {
                    m_time = DateTime.Now;
                    return (Sentences.OnlyHiraganaKatakanaRomaji(m_guild.Id));
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
                    string hiragana = Features.Tools.Linguist.ToHiragana(Utilities.GetElementXml("\"reading\":\"", s, '"'));
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
                    m_time = DateTime.Now;
                    return (Sentences.ShiritoriDoesntExist(m_guild.Id));
                }
                if (!isNoun)
                {
                    m_time = DateTime.Now;
                    return (Sentences.ShiritoriNotNoun(m_guild.Id));
                }
                if (userWord[0] != HiraganaToUpper(m_currWord[m_currWord.Length - 1]))
                {
                    m_time = DateTime.Now;
                    return (Sentences.ShiritoriMustBegin(m_guild.Id, HiraganaToUpper(m_currWord[m_currWord.Length - 1]).ToString(), Features.Tools.Linguist.ToRomaji(m_currWord[m_currWord.Length - 1].ToString())));
                }
                if (m_alreadySaid.Contains(userWord))
                {
                    m_didLost = true;
                    return (Sentences.ShiritoriAlreadySaid(m_guild.Id));
                }
                if (userWord[userWord.Length - 1] == 'ん')
                {
                    m_didLost = true;
                    return (Sentences.ShiritoriEndWithN(m_guild.Id));
                }
                m_words.Remove(m_words.Find(x => x.Split('$')[0] == userWord));
                m_alreadySaid.Add(userWord);
                m_currWord = userWord;
                return (null);
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
                    finalStr += Sentences.ShiritoriSuggestion(m_guild.Id, insideWord[0], Features.Tools.Linguist.ToRomaji(insideWord[0]), insideWord[1]) + Environment.NewLine;
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
                    json = json.Split(new string[] { "List of destroyers" }, StringSplitOptions.None)[1].Split(new string[] { "=={{anchor|type}}" }, StringSplitOptions.None)[0];
                    m_shipNames = new List<string>();
                    MatchCollection matches = Regex.Matches(json, @"\[\[([A-Za-z- 0-9]+)");
                    foreach (Match match in matches)
                    {
                        string str = match.Groups[1].Value;
                        if (!str.StartsWith("List of") && !str.StartsWith("Auxiliary"))
                            m_shipNames.Add(str);
                    }
                }
                m_toGuess = null;
                m_idImage = "-1";
            }

            public override string[] GetPost() // TODO: sometimes post wrong images
            {
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
                    image = image.Split(new string[] { "/revision" }, StringSplitOptions.None)[0].Replace("\\", "");
                    return (new string[] { image });
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
                    using (WebClient w = new WebClient())
                    {
                        string url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + newName + "&limit=1";
                        string json = w.DownloadString(url);
                        string code = Utilities.GetElementXml("\"title\":\"", json, '"');
                        url = "http://kancolle.wikia.com/wiki/" + code + "?action=raw";
                        url = url.Replace(' ', '_');
                        json = w.DownloadString(url);
                        if (Utilities.GetElementXml("{{", json, '}') != "ShipPageHeader")
                            return (Sentences.KancolleGuessDontExist(m_guild.Id));
                        w.Encoding = Encoding.UTF8;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        url = "https://kancolle.wikia.com/api/v1/Search/List?query=" + newName + "&limit=1";
                        json = w.DownloadString(url);
                        code = Utilities.GetElementXml("\"id\":", json, ',');
                        if (m_idImage == code)
                            return (null);
                        return (Sentences.GuessBad(m_guild.Id, newName));
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
                m_booru = new Gelbooru();
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

            public override string GetCheckCorrect(string userWord, out bool sayCorrect)
            {
                sayCorrect = true;
                m_nbAttempt++;
                if (Utilities.CleanWord(userWord) == Utilities.CleanWord(m_toGuess))
                    return (null);
                if (Utilities.CleanWord(userWord) != "" && (Utilities.CleanWord(m_toGuess).Contains(Utilities.CleanWord(userWord)) || Utilities.CleanWord(userWord).Contains(Utilities.CleanWord(m_toGuess))))
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
            public AnimeGame(IMessageChannel chan, IGuild guild, IUser charac, bool isEasy) : base(chan, guild, charac, animeTimer, "anime.dat", isEasy)
            {
                m_toGuess = null;
                m_allTags = new List<string>();
                string[] allLines = File.ReadAllLines("Saves/AnimeTags.dat");
                foreach (string line in allLines)
                    m_allTags.Add(line.Split(' ')[0]);
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

        [Command("Play", RunMode = RunMode.Async), Summary("Launch a game")]
        public async Task PlayShiritori(params string[] gameName)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (p.games.Any(x => x.m_chan == Context.Channel))
                await ReplyAsync(Sentences.GameAlreadyRunning(Context.Guild.Id));
            else if (gameName.Length == 0)
                await ReplyAsync(Sentences.InvalidGameName(Context.Guild.Id));
            else
            {
                if (gameName[0].ToLower() != "shiritori" && gameName[0].ToLower() != "kancolle" && gameName[0].ToLower() != "booru" && gameName[0].ToLower() != "anime")
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
                        if (!File.Exists("Saves/shiritoriWords.dat"))
                        {
                            await ReplyAsync(Base.Sentences.NoDictionnary(Context.Guild.Id));
                            return;
                        }
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
                            await ReplyAsync(Base.Sentences.ChanIsNotNsfw(Context.Guild.Id));
                            return;
                        }
                        if (!File.Exists("Saves/BooruTriviaTags.dat"))
                        {
                            await ReplyAsync(Base.Sentences.NoDictionnary(Context.Guild.Id));
                            return;
                        }
                        await ReplyAsync(Sentences.RulesBooru(Context.Guild.Id));
                        g = new BooruGame(Context.Channel, Context.Guild, Context.User, isEasy);
                    }
                    else if (gameName[0].ToLower() == "anime")
                    {
                        if (!File.Exists("Saves/AnimeTags.dat"))
                        {
                            await ReplyAsync(Base.Sentences.NoDictionnary(Context.Guild.Id));
                            return;
                        }
                        await ReplyAsync(Sentences.RulesAnime(Context.Guild.Id));
                        g = new AnimeGame(Context.Channel, Context.Guild, Context.User, isEasy);
                    }
                    if (Program.p.sendStats)
                        await Program.p.UpdateElement(new Tuple<string, string>[] { new Tuple<string, string>("games", gameName[0].ToLower()) });
                    p.games.Add(g);
                    await g.Post();
                }
            }
        }
    }
}