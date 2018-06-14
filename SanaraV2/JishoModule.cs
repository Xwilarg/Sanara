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
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Google;
using Google.Cloud.Translation.V2;

namespace SanaraV2
{
    public class LinguistModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Hiragana"), Summary("To hiragana")]
        public async Task toHiraganaCmd(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.toHiraganaHelp(Context.Guild.Id));
            else
                await ReplyAsync(toHiragana(fromKatakana(Program.addArgs(word))));
        }

        [Command("Romaji"), Summary("To romaji")]
        public async Task toRomajiCmd(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.toRomajiHelp(Context.Guild.Id));
            else
                await ReplyAsync(fromKatakana(fromHiragana(Program.addArgs(word))));
        }

        [Command("Katakana"), Summary("To romaji")]
        public async Task toKatakanaCmd(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.toKatakanaHelp(Context.Guild.Id));
            else
                await ReplyAsync(toKatakana(fromHiragana(Program.addArgs(word))));
        }

        [Command("Translation"), Summary("Translate a sentence")]
        public async Task translation(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (p.translationClient == null)
                await ReplyAsync(Sentences.noApiKey(Context.Guild.Id));
            else if (words.Length < 2)
                await ReplyAsync(Sentences.translateHelp(Context.Guild.Id));
            else
            {
                string language;
                if (words[0].Length == 2)
                    language = words[0];
                else if (words[0].ToLower() == "french" || words[0].ToLower() == "français")
                    language = "fr";
                else if (words[0].ToLower() == "japanese" || words[0].ToLower() == "日本語")
                    language = "ja";
                else if (words[0].ToLower() == "english")
                    language = "en";
                else if (words[0].ToLower() == "spanish" || words[0].ToLower() == "español")
                    language = "es";
                else if (words[0].ToLower() == "german" || words[0].ToLower() == "deutsch")
                    language = "de";
                else
                {
                    await ReplyAsync(Sentences.invalidLanguage(Context.Guild.Id));
                    return;
                }
                List<string> newWords = words.ToList();
                newWords.RemoveAt(0);
                try
                {
                    string sourceLanguage;
                    string translation = getTranslation(Program.addArgs(newWords.ToArray()), language, out sourceLanguage);
                    if (sourceLanguage == "en") sourceLanguage = "english";
                    else if (sourceLanguage == "fr") sourceLanguage = "french";
                    else if (sourceLanguage == "ja") sourceLanguage = "japanese";
                    else if (sourceLanguage == "es") sourceLanguage = "spanish";
                    else if (sourceLanguage == "de") sourceLanguage = "german";
                    await ReplyAsync("From " + sourceLanguage + ":" + Environment.NewLine + "```" + Environment.NewLine + translation + Environment.NewLine + "```");
                }
                catch (GoogleApiException)
                {
                    await ReplyAsync(Sentences.invalidLanguage(Context.Guild.Id));
                    return;
                }
            }
        }

        [Command("Definition", RunMode = RunMode.Async), Summary("Give the meaning of a word")]
        public async Task meaning(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.japaneseHelp(Context.Guild.Id));
            else
            {
                foreach (string s in getAllKanjis(Program.addArgs(word), 0))
                {
                    await ReplyAsync(s);
                }
            }
        }

        public static string getTranslation(string words, string language, out string sourceLanguage)
        {
            TranslationResult translation = Program.p.translationClient.TranslateText(words, language);
            sourceLanguage = translation.DetectedSourceLanguage;
            return (translation.TranslatedText);
        }

        public static List<string> getAllKanjis(string word, ulong guildId)
        {
            string newWord = word.Replace(" ", "%20");
            string json;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + newWord.ToLower());
            }
            string[] url = Program.getElementXml("\"japanese\":[", json, '$').Split(new string[] { "\"japanese\":[" }, StringSplitOptions.None);
            string finalStr = Sentences.giveJapaneseTranslations(guildId, word) + Environment.NewLine + Environment.NewLine;
            if (url[0] == "")
                return new List<string>() { Sentences.noJapaneseTranslation(guildId, word) + "." };
            else
            {
                List<string> finalList = new List<string>();
                int counter = 0;
                foreach (string str in url)
                {
                    string[] urlResult = str.Split(new string[] { "},{" }, StringSplitOptions.None);
                    string[] meanings = Program.getElementXml("english_definitions\":[", str, ']').Split(new string[] { "\",\"" }, StringSplitOptions.None);
                    foreach (string s in urlResult)
                    {
                        if (Program.getElementXml("\"reading\":\"", s, '"') == "" && Program.getElementXml("\"word\":\"", s, '"') == "")
                            continue;
                        finalStr += ((Program.getElementXml("\"word\":\"", s, '"') == "") ? (Program.getElementXml("\"reading\":\"", s, '"') + " (" + fromKatakana(fromHiragana(Program.getElementXml("\"reading\":\"", s, '"') + ")")))
                        : (Program.getElementXml("\"word\":\"", s, '"')
                        + ((Program.getElementXml("\"reading\":\"", s, '"') == "") ? ("") : (" (" + Program.getElementXml("\"reading\":\"", s, '"') + " - " + fromHiragana(fromKatakana(Program.getElementXml("\"reading\":\"", s, '"') + ")")))))) + Environment.NewLine;
                    }
                    finalStr += Sentences.meaning(guildId);
                    string allMeanings = "";
                    foreach (string sm in meanings)
                    {
                        allMeanings += sm + " / ";
                    }
                    finalStr += allMeanings.Substring(1, allMeanings.Length - 5);
                    if (counter == 4)
                        break;
                    else
                    {
                        if (finalStr.Length > 1250)
                        {
                            finalList.Add(finalStr);
                        }
                        else
                            finalStr += Environment.NewLine + Environment.NewLine;
                    }
                    counter++;
                }
                finalList.Add(finalStr);
                return (finalList);
            }
        }

        private static string Transcript(char curr, char next, ref int i, Dictionary<string, string> transcriptionArray)
        {
            if (next != ' ' && transcriptionArray.ContainsKey("" + curr + next))
            {
                i++;
                return (transcriptionArray["" + curr + next]);
            }
            if (transcriptionArray.ContainsKey("" + curr))
                return (transcriptionArray["" + curr]);
            return ("" + curr);
        }

        public static string fromHiragana(string name)
        {
            string finalName = "";
            string finalStr = "";
            int doubleVoy = 0;
            Dictionary<string, string> transcriptionArray = new Dictionary<string, string>()
            {
                { "あ", "a" },
                { "い", "i" },
                { "う", "u" },
                { "え", "e" },
                { "お", "o" },
                { "か", "ka" },
                { "きゃ", "kya" },
                { "きぃ", "kyi" },
                { "きゅ", "kyu" },
                { "きぇ", "kye" },
                { "きょ", "kyo" },
                { "き", "ki" },
                { "く", "ku" },
                { "け", "ke" },
                { "こ", "ko" },
                { "が", "ga" },
                { "ぎゃ", "gya" },
                { "ぎぃ", "gyi" },
                { "ぎゅ", "gyu" },
                { "ぎぇ", "gye" },
                { "ぎょ", "gyo" },
                { "ぎ", "gi" },
                { "ぐ", "gu" },
                { "げ", "ge" },
                { "ご", "go" },
                { "さ", "sa" },
                { "しゃ", "sha" },
                { "しゅ", "shu" },
                { "しぇ", "she" },
                { "しょ", "sho" },
                { "し", "shi" },
                { "す", "su" },
                { "せ", "se" },
                { "そ", "so" },
                { "ざ", "za" },
                { "じゃ", "dja" },
                { "じぃ", "dji" },
                { "じゅ", "dju" },
                { "じぇ", "dje" },
                { "じょ", "djo" },
                { "じ", "dji" },
                { "ず", "zu" },
                { "ぜ", "ze" },
                { "ぞ", "zo" },
                { "た", "ta" },
                { "ちゃ", "cha" },
                { "ちぃ", "chi" },
                { "ちゅ", "chu" },
                { "ちぇ", "che" },
                { "ちょ", "cho" },
                { "ち", "chi" },
                { "つ", "tsu" },
                { "て", "te" },
                { "と", "to" },
                { "だ", "da" },
                { "ぢゃ", "dja" },
                { "ぢぃ", "dji" },
                { "ぢゅ", "dju" },
                { "ぢぇ", "dje" },
                { "ぢょ", "djo" },
                { "ぢ", "dji" },
                { "づ", "dzu" },
                { "で", "de" },
                { "ど", "do" },
                { "な", "na" },
                { "にゃ", "nya" },
                { "にぃ", "nyi" },
                { "にゅ", "nyu" },
                { "にぇ", "nye" },
                { "にょ", "nyo" },
                { "に", "ni" },
                { "ぬ", "nu" },
                { "ね", "ne" },
                { "の", "no" },
                { "は", "ha" },
                { "ひゃ", "hya" },
                { "ひぃ", "hyi" },
                { "ひゅ", "hyu" },
                { "ひぇ", "hye" },
                { "ひょ", "hyo" },
                { "ひ", "hi" },
                { "ふ", "fu" },
                { "へ", "he" },
                { "ほ", "ho" },
                { "ば", "ba" },
                { "びゃ", "bya" },
                { "びぃ", "byi" },
                { "びゅ", "byu" },
                { "びぇ", "bye" },
                { "びょ", "byo" },
                { "び", "bi" },
                { "ぶ", "bu" },
                { "べ", "be" },
                { "ぼ", "bo" },
                { "ぱ", "pa" },
                { "ぴゃ", "pya" },
                { "ぴぃ", "pyi" },
                { "ぴゅ", "pyu" },
                { "ぴぇ", "pye" },
                { "ぴょ", "pyo" },
                { "ぴ", "pi" },
                { "ぷ", "pu" },
                { "ぺ", "pe" },
                { "ぽ", "po" },
                { "ま", "ma" },
                { "みゃ", "mya" },
                { "みぃ", "myi" },
                { "みゅ", "myu" },
                { "みぇ", "mye" },
                { "みょ", "myo" },
                { "み", "mi" },
                { "む", "mu" },
                { "め", "me" },
                { "も", "mo" },
                { "や", "ya" },
                { "ゆ", "yu" },
                { "いぇ", "ye" },
                { "よ", "yo" },
                { "ら", "ra" },
                { "りゃ", "rya" },
                { "りぃ", "ryi" },
                { "りゅ", "ryu" },
                { "りぇ", "rye" },
                { "りょ", "ryo" },
                { "り", "ri" },
                { "る", "ru" },
                { "れ", "re" },
                { "ろ", "ro" },
                { "わ", "wa" },
                { "ゐ", "wi" },
                { "ゑ", "we" },
                { "を", "wo" },
                { "ゔ", "vu" },
                { "ん", "n" }
            };
            for (int i = 0; i < name.Length; i++)
            {
                finalName = "";
                doubleVoy--;
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                if (curr == 'っ')
                    doubleVoy = 2;
                else
                    finalName += Transcript(curr, next, ref i, transcriptionArray);
                if (doubleVoy == 1 && curr != 'ん' && curr != 'ゔ' && curr != 'ゃ' && curr != 'ぃ' && curr != 'ゅ' && curr != 'ぇ' && curr != 'ょ'
                     && curr != 'っ' && curr != 'あ' && curr != 'い' && curr != 'う' && curr != 'え' && curr != 'お')
                {
                    finalName = finalName[0] + finalName;
                }
                finalStr += finalName;
                finalName = "";
            }
            finalStr += finalName;
            return (finalStr);
        }

        public static string fromKatakana(string name)
        {
            string finalName = "";
            string finalStr = "";
            int doubleVoy = 0;
            Dictionary<string, string> transcriptionArray = new Dictionary<string, string>()
            {
                { "ア", "a" },
                { "イ", "i" },
                { "ウ", "u" },
                { "エ", "e" },
                { "オ", "o" },
                { "カ", "ka" },
                { "キャ", "kya" },
                { "キィ", "kyi" },
                { "キュ", "kyu" },
                { "キェ", "kye" },
                { "キョ", "kyo" },
                { "キ", "ki" },
                { "ク", "ku" },
                { "ケ", "ke" },
                { "コ", "ko" },
                { "ガ", "ga" },
                { "ギャ", "gya" },
                { "ギィ", "gyi" },
                { "ギュ", "gyu" },
                { "ギェ", "gye" },
                { "ギョ", "gyo" },
                { "ギ", "gi" },
                { "グ", "gu" },
                { "ゲ", "ge" },
                { "ゴ", "go" },
                { "サ", "sa" },
                { "シャ", "sha" },
                { "シュ", "shu" },
                { "シェ", "she" },
                { "ショ", "sho" },
                { "シ", "shi" },
                { "ス", "su" },
                { "セ", "se" },
                { "ソ", "so" },
                { "ザ", "za" },
                { "ジャ", "dja" },
                { "ジィ", "dji" },
                { "ジュ", "dju" },
                { "ジェ", "dje" },
                { "ジョ", "djo" },
                { "ジ", "dji" },
                { "ズ", "zu" },
                { "ゼ", "ze" },
                { "ゾ", "zo" },
                { "タ", "ta" },
                { "チャ", "cha" },
                { "チィ", "chi" },
                { "チュ", "chu" },
                { "チェ", "che" },
                { "チョ", "cho" },
                { "チ", "chi" },
                { "ツ", "tsu" },
                { "テ", "te" },
                { "ト", "to" },
                { "ダ", "da" },
                { "ヅ", "dzu" },
                { "デ", "de" },
                { "ド", "do" },
                { "ナ", "na" },
                { "ニャ", "nya" },
                { "ニィ", "nyi" },
                { "ニュ", "nyu" },
                { "ニェ", "nye" },
                { "ニョ", "nyo" },
                { "ニ", "ni" },
                { "ヌ", "nu" },
                { "ネ", "ne" },
                { "ノ", "no" },
                { "ハ", "ha" },
                { "ヒャ", "hya" },
                { "ヒィ", "hyi" },
                { "ヒュ", "hyu" },
                { "ヒェ", "hye" },
                { "ヒョ", "hyo" },
                { "ヒ", "hi" },
                { "フ", "hu" },
                { "ヘ", "he" },
                { "ホ", "ho" },
                { "バ", "ba" },
                { "ビャ", "bya" },
                { "ビィ", "byi" },
                { "ビュ", "byu" },
                { "ビェ", "bye" },
                { "ビョ", "byo" },
                { "ビ", "bi" },
                { "ブ", "bu" },
                { "ベ", "be" },
                { "ボ", "bo" },
                { "パ", "pa" },
                { "ピャ", "pya" },
                { "ピィ", "pyi" },
                { "ピュ", "pyu" },
                { "ピェ", "pye" },
                { "ピョ", "pyo" },
                { "ピ", "pi" },
                { "プ", "pu" },
                { "ペ", "pe" },
                { "ポ", "po" },
                { "マ", "ma" },
                { "ミャ", "mya" },
                { "ミィ", "myi" },
                { "ミュ", "myu" },
                { "ミェ", "mye" },
                { "ミョ", "myo" },
                { "ミ", "mi" },
                { "ム", "mu" },
                { "メ", "me" },
                { "モ", "mo" },
                { "ヤ", "ya" },
                { "イェ", "ye" },
                { "ユ", "yu" },
                { "ヨ", "yo" },
                { "ラ", "ra" },
                { "リャ", "rya" },
                { "リィ", "ryi" },
                { "リュ", "ryu" },
                { "リェ", "rye" },
                { "リョ", "ryo" },
                { "リ", "ri" },
                { "ル", "ru" },
                { "レ", "re" },
                { "ロ", "ro" },
                { "ワ", "wa" },
                { "ウィ", "wi" },
                { "ウェ", "we" },
                { "ヲ", "wo" },
                { "ヴァ", "va" },
                { "ヴィ", "vi" },
                { "ヴェ", "ve" },
                { "ヴォ", "vo" },
                { "ヴ", "vu" },
                { "ン", "n" }
            };
            for (int i = 0; i < name.Length; i++)
            {
                finalName = "";
                doubleVoy--;
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                if (curr == 'ッ')
                    doubleVoy = 2;
                else if (curr == 'ー' && finalStr.Length > 0)
                    finalName += finalStr.Substring(finalStr.Length - 1, 1);
                else
                    finalName += Transcript(curr, next, ref i, transcriptionArray);
                if (doubleVoy == 1 && curr != 'ン' && curr != 'ヴ' && curr != 'ャ' && curr != 'ィ' && curr != 'ュ' && curr != 'ェ' && curr != 'ョ'
                     && curr != 'ッ' && curr != 'ア' && curr != 'イ' && curr != 'ウ' && curr != 'エ' && curr != 'オ')
                {
                    finalName = finalName[0] + finalName;
                }
                finalStr += finalName;
                finalName = "";
            }
            finalStr += finalName;
            return (finalStr);
        }

        private static string TranscriptInvert(char curr, char next, char nnext, ref int i, Dictionary<string, string> transcriptionArray)
        {
            if (next != ' ')
            {
                if (nnext != ' ' && transcriptionArray.ContainsKey("" + curr + next + nnext))
                {
                    i++;
                    return (transcriptionArray["" + curr + next + nnext]);
                }
                if (transcriptionArray.ContainsKey("" + curr + next))
                    return (transcriptionArray["" + curr + next]);
            }
            i--;
            if (transcriptionArray.ContainsKey("" + curr))
                return (transcriptionArray["" + curr]);
            return ("" + curr);
        }

        public static string toHiragana(string name)
        {
            string finalName = "";
            name = name.ToLower();
            Dictionary<string, string> transcriptionArray = new Dictionary<string, string>()
            {
                { "a", "あ" },
                { "i", "い" },
                { "u", "う" },
                { "e", "え" },
                { "o", "お" },
                { "ka", "か" },
                { "kya", "きゃ" },
                { "kyi", "きぃ" },
                { "kyu", "きゅ" },
                { "kye", "きぇ" },
                { "kyo", "じょ" },
                { "ki", "き" },
                { "ku", "く" },
                { "ke", "け" },
                { "ko", "こ" },
                { "ga", "が" },
                { "gya", "ぎゃ" },
                { "gyi", "ぎぃ" },
                { "gyu", "ぎゅ" },
                { "gye", "ぎぇ" },
                { "gyo", "ぎょ" },
                { "gi", "ぎ" },
                { "gu", "ぐ" },
                { "ge", "げ" },
                { "go", "ご" },
                { "sa", "さ" },
                { "shi", "し" },
                { "sha", "しゃ" },
                { "shu", "しゅ" },
                { "she", "しぇ" },
                { "sho", "しょ" },
                { "su", "す" },
                { "se", "せ" },
                { "so", "そ" },
                { "za", "ざ" },
                { "ji", "じ" },
                { "dji", "じ" },
                { "jya", "じゃ" },
                { "jyi", "じぃ" },
                { "jyu", "じゅ" },
                { "jye", "じぇ" },
                { "jyo", "じょ" },
                { "ja", "じゃ" },
                { "ju", "じゅ" },
                { "je", "じぇ" },
                { "jo", "じょ" },
                { "dja", "じゃ" },
                { "dju", "じゅ" },
                { "dje", "じぇ" },
                { "djo", "じょ" },
                { "zu", "ず" },
                { "ze", "ぜ" },
                { "zo", "ぞ" },
                { "ta", "た" },
                { "tya", "ちゃ" },
                { "tyi", "ちぃ" },
                { "tyu", "ちゅ" },
                { "tye", "ちぇ" },
                { "tyo", "ちょ" },
                { "chi", "ち" },
                { "cha", "ちゃ" },
                { "chu", "ちゅ" },
                { "che", "ちぇ" },
                { "cho", "ちょ" },
                { "tsu", "つ" },
                { "te", "て" },
                { "to", "と" },
                { "dya", "ぢゃ" },
                { "dyi", "ぢぃ" },
                { "dyu", "ぢゅ" },
                { "dye", "ぢぇ" },
                { "dyo", "ぢょ" },
                { "dzu", "づ" },
                { "de", "で" },
                { "do", "ど" },
                { "na", "な" },
                { "nya", "にゃ" },
                { "nyi", "にぃ" },
                { "nyu", "にゅ" },
                { "nye", "にぇ" },
                { "nyo", "にょ" },
                { "ni", "に" },
                { "nu", "ぬ" },
                { "ne", "ね" },
                { "no", "の" },
                { "ha", "は" },
                { "hya", "ひゃ" },
                { "hyi", "ひぃ" },
                { "hyu", "ひゅ" },
                { "hye", "ひぇ" },
                { "hyo", "ひょ" },
                { "hi", "ひ" },
                { "fu", "ふ" },
                { "he", "へ" },
                { "ho", "ほ" },
                { "ba", "ば" },
                { "bya", "びゃ" },
                { "byi", "びぃ" },
                { "byu", "びゅ" },
                { "bye", "びぇ" },
                { "byo", "びょ" },
                { "bi", "び" },
                { "bu", "ぶ" },
                { "be", "べ" },
                { "bo", "ぼ" },
                { "pa", "ぱ" },
                { "pya", "ぴゃ" },
                { "pyi", "ぴぃ" },
                { "pyu", "ぴゅ" },
                { "pye", "ぴぇ" },
                { "pyo", "ぴょ" },
                { "pi", "ぴ" },
                { "pu", "ぷ" },
                { "pe", "ぺ" },
                { "po", "ぽ" },
                { "ma", "ま" },
                { "mya", "みゃ" },
                { "myi", "みぃ" },
                { "myu", "みゅ" },
                { "mye", "みぇ" },
                { "myo", "みょ" },
                { "mi", "み" },
                { "mu", "む" },
                { "me", "め" },
                { "mo", "も" },
                { "ya", "や" },
                { "yu", "ゆ" },
                { "yo", "よ" },
                { "ra", "ら" },
                { "rya", "りゃ" },
                { "ryi", "りぃ" },
                { "ryu", "りゅ" },
                { "rye", "りぇ" },
                { "ryo", "りょ" },
                { "ri", "り" },
                { "ru", "る" },
                { "re", "れ" },
                { "ro", "ろ" },
                { "wa", "わ" },
                { "wi", "ゐ" },
                { "we", "ゑ" },
                { "wo", "を" },
                { "vu", "ゔ" },
                { "n", "ん" }
            };
            for (int i = 0; i < name.Length; i += 2)
            {
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                char nnext = ((i < name.Length - 2) ? (name[i + 2]) : (' '));
                if (curr != 'a' && curr != 'i' && curr != 'u' && curr != 'e' && curr != 'o' && curr != 'n'
                    && curr == next)
                {
                    finalName += "っ";
                    i--;
                }
                else
                    finalName += TranscriptInvert(curr, next, nnext, ref i, transcriptionArray);
            }
            return (finalName);
        }

        public static string toKatakana(string name)
        {
            string finalName = "";
            name = name.ToLower();
            Dictionary<string, string> transcriptionArray = new Dictionary<string, string>()
            {
                { "a", "ア" },
                { "i", "イ" },
                { "u", "ウ" },
                { "e", "エ" },
                { "o", "オ" },
                { "ka", "カ" },
                { "kya", "キャ" },
                { "kyi", "キィ" },
                { "kyu", "キュ" },
                { "kye", "キェ" },
                { "kyo", "キョ" },
                { "ki", "キ" },
                { "ku", "ク" },
                { "ke", "ケ" },
                { "ko", "コ" },
                { "ga", "ガ" },
                { "gya", "ギャ" },
                { "gyi", "ギィ" },
                { "gyu", "ギュ" },
                { "gye", "ギェ" },
                { "gyo", "ギョ" },
                { "gi", "ギ" },
                { "gu", "グ" },
                { "ge", "ゲ" },
                { "go", "ゴ" },
                { "sa", "サ" },
                { "shi", "シ" },
                { "sha", "シャ" },
                { "shu", "シュ" },
                { "she", "シェ" },
                { "sho", "ショ" },
                { "su", "ス" },
                { "se", "セ" },
                { "so", "ソ" },
                { "za", "ザ" },
                { "ji", "ジ" },
                { "dji", "ジ" },
                { "jya", "ジャ" },
                { "jyi", "ジィ" },
                { "jyu", "ジュ" },
                { "jye", "ジェ" },
                { "jyo", "ジョ" },
                { "ja", "ジャ" },
                { "ju", "ジュ" },
                { "je", "ジェ" },
                { "jo", "ジョ" },
                { "dja", "ジャ" },
                { "dju", "ジュ" },
                { "dje", "ジェ" },
                { "djo", "ジョ" },
                { "zu", "ズ" },
                { "ze", "ゼ" },
                { "zo", "ゾ" },
                { "ta", "タ" },
                { "tya", "チャ" },
                { "tyi", "チィ" },
                { "tyu", "チュ" },
                { "tye", "チェ" },
                { "tyo", "チョ" },
                { "chi", "チ" },
                { "cha", "チャ" },
                { "chu", "チュ" },
                { "che", "チェ" },
                { "cho", "チョ" },
                { "tsu", "ツ" },
                { "te", "テ" },
                { "to", "ト" },
                { "dya", "ヂャ" },
                { "dyi", "ヂィ" },
                { "dyu", "ヂュ" },
                { "dye", "ヂェ" },
                { "dyo", "ヂョ" },
                { "dzu", "ズ" },
                { "de", "デ" },
                { "do", "ド" },
                { "na", "ナ" },
                { "nya", "ニャ" },
                { "nyi", "ニィ" },
                { "nyu", "ニュ" },
                { "nye", "ニェ" },
                { "nyo", "ニョ" },
                { "ni", "ニ" },
                { "nu", "ヌ" },
                { "ne", "ネ" },
                { "no", "ノ" },
                { "ha", "ハ" },
                { "hya", "ヒャ" },
                { "hyi", "ヒィ" },
                { "hyu", "ヒュ" },
                { "hye", "ヒェ" },
                { "hyo", "ヒョ" },
                { "hi", "ヒ" },
                { "fu", "フ" },
                { "he", "ヘ" },
                { "ho", "ホ" },
                { "ba", "バ" },
                { "bya", "ビャ" },
                { "byi", "ビィ" },
                { "byu", "ビュ" },
                { "bye", "ビェ" },
                { "byo", "ビョ" },
                { "bi", "ビ" },
                { "bu", "ブ" },
                { "be", "ベ" },
                { "bo", "ボ" },
                { "pa", "パ" },
                { "pya", "ピャ" },
                { "pyi", "ピィ" },
                { "pyu", "ピュ" },
                { "pye", "ピェ" },
                { "pyo", "ピョ" },
                { "pi", "ピ" },
                { "pu", "プ" },
                { "pe", "ペ" },
                { "po", "ポ" },
                { "ma", "マ" },
                { "mya", "ミャ" },
                { "myi", "ミィ" },
                { "myu", "ミュ" },
                { "mye", "ミェ" },
                { "myo", "ミョ" },
                { "mi", "ミ" },
                { "mu", "ム" },
                { "me", "メ" },
                { "mo", "モ" },
                { "ya", "ヤ" },
                { "yu", "ユ" },
                { "yo", "ヨ" },
                { "ra", "ラ" },
                { "rya", "リャ" },
                { "ryi", "リィ" },
                { "ryu", "リュ" },
                { "rye", "リェ" },
                { "ryo", "リョ" },
                { "ri", "リ" },
                { "ru", "ル" },
                { "re", "レ" },
                { "ro", "ロ" },
                { "wa", "ワ" },
                { "wi", "ウィ" },
                { "we", "ウェ" },
                { "wo", "ヲ" },
                { "va", "ヴァ" },
                { "vi", "ヴィ" },
                { "ve", "ヴェ" },
                { "vo", "ヴォ" },
                { "vu", "ヴ" },
                { "n", "ン" }
            };
            for (int i = 0; i < name.Length; i += 2)
            {
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                char nnext = ((i < name.Length - 2) ? (name[i + 2]) : (' '));
                if (curr != 'a' && curr != 'i' && curr != 'u' && curr != 'e' && curr != 'o' && curr != 'n'
                    && curr == next)
                {
                    finalName += "ッ";
                    i--;
                }
                else
                    finalName += TranscriptInvert(curr, next, nnext, ref i, transcriptionArray);
            }
            return (finalName);
        }
    }
}