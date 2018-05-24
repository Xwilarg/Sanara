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
                await ReplyAsync(Sentences.toHiraganaHelp);
            else
                await ReplyAsync(toHiragana(fromKatakana(Program.addArgs(word))));
        }

        [Command("Romaji"), Summary("To romaji")]
        public async Task toRomajiCmd(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.toRomajiHelp);
            else
                await ReplyAsync(fromKatakana(fromHiragana(Program.addArgs(word))));
        }

        [Command("Katakana"), Summary("To romaji")]
        public async Task toKatakanaCmd(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.toKatakanaHelp);
            else
                await ReplyAsync(toKatakana(fromHiragana(Program.addArgs(word))));
        }

        [Command("Translation"), Summary("Translate a sentence")]
        public async Task translation(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (words.Length < 2)
                await ReplyAsync(Sentences.translateHelp);
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
                    await ReplyAsync(Sentences.invalidLanguage);
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
                    await ReplyAsync(Sentences.invalidLanguage);
                    return;
                }
            }
        }

        [Command("Definition", RunMode = RunMode.Async), Summary("Give the meaning of a word")]
        public async Task meaning(params string[] word)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.japaneseHelp);
            else
            {
                foreach (string s in getAllKanjis(Program.addArgs(word)))
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

        public static List<string> getAllKanjis(string word)
        {
            string newWord = word.Replace(" ", "%20");
            string json;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + newWord.ToLower());
            }
            string[] url = Program.getElementXml("\"japanese\":[", json, '$').Split(new string[] { "\"japanese\":[" }, StringSplitOptions.None);
            string finalStr = "Here are the japanese translations for " + word + ":" + Environment.NewLine + Environment.NewLine;
            if (url[0] == "")
                return new List<string>() { "I didn't find any japanase translation for " + word + "." };
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
                    finalStr += "Meaning: ";
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

        public static string fromHiragana(string name)
        {
            string finalName = "";
            string finalStr = "";
            int doubleVoy = 0;
            for (int i = 0; i < name.Length; i++)
            {
                finalName = "";
                doubleVoy--;
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                char nnext = ((i < name.Length - 2) ? (name[i + 2]) : (' '));
                if (curr == 'っ')
                {
                    doubleVoy = 2;
                }
                else if (curr == 'あ') finalName += 'a';
                else if (curr == 'い') finalName += 'i';
                else if (curr == 'う') finalName += 'u';
                else if (curr == 'え') finalName += 'e';
                else if (curr == 'お') finalName += 'o';
                else if (curr == 'か') finalName += "ka";
                else if (curr == 'き' && next == 'ゃ') { finalName += "kya"; i++; }
                else if (curr == 'き' && next == 'ぃ') { finalName += "kyi"; i++; }
                else if (curr == 'き' && next == 'ゅ') { finalName += "kyu"; i++; }
                else if (curr == 'き' && next == 'ぇ') { finalName += "kye"; i++; }
                else if (curr == 'き' && next == 'ょ') { finalName += "kyo"; i++; }
                else if (curr == 'き') finalName += "ki";
                else if (curr == 'く') finalName += "ku";
                else if (curr == 'け') finalName += "ke";
                else if (curr == 'こ') finalName += "ko";
                else if (curr == 'が') finalName += "ga";
                else if (curr == 'ぎ' && next == 'ゃ') { finalName += "gya"; i++; }
                else if (curr == 'ぎ' && next == 'ぃ') { finalName += "gyi"; i++; }
                else if (curr == 'ぎ' && next == 'ゅ') { finalName += "gyu"; i++; }
                else if (curr == 'ぎ' && next == 'ぇ') { finalName += "gye"; i++; }
                else if (curr == 'ぎ' && next == 'ょ') { finalName += "gyo"; i++; }
                else if (curr == 'ぎ') finalName += "gi";
                else if (curr == 'ぐ') finalName += "gu";
                else if (curr == 'げ') finalName += "ge";
                else if (curr == 'ご') finalName += "go";
                else if (curr == 'さ') finalName += "sa";
                else if (curr == 'し' && next == 'ゃ') { finalName += "sha"; i++; }
                else if (curr == 'し' && next == 'ゅ') { finalName += "shu"; i++; }
                else if (curr == 'し' && next == 'ぇ') { finalName += "she"; i++; }
                else if (curr == 'し' && next == 'ょ') { finalName += "sho"; i++; }
                else if (curr == 'し') finalName += "shi";
                else if (curr == 'す') finalName += "su";
                else if (curr == 'せ') finalName += "se";
                else if (curr == 'そ') finalName += "so";
                else if (curr == 'ざ') finalName += "za";
                else if (curr == 'じ' && next == 'ゃ') { finalName += "dja"; i++; }
                else if (curr == 'じ' && next == 'ぃ') { finalName += "dji"; i++; }
                else if (curr == 'じ' && next == 'ゅ') { finalName += "dju"; i++; }
                else if (curr == 'じ' && next == 'ぇ') { finalName += "dje"; i++; }
                else if (curr == 'じ' && next == 'ょ') { finalName += "djo"; i++; }
                else if (curr == 'じ') finalName += "dji";
                else if (curr == 'ず') finalName += "zu";
                else if (curr == 'ぜ') finalName += "ze";
                else if (curr == 'ぞ') finalName += "zo";
                else if (curr == 'た') finalName += "ta";
                else if (curr == 'ち' && next == 'ゃ') { finalName += "cha"; i++; }
                else if (curr == 'ち' && next == 'ぃ') { finalName += "chi"; i++; }
                else if (curr == 'ち' && next == 'ゅ') { finalName += "chu"; i++; }
                else if (curr == 'ち' && next == 'ぇ') { finalName += "che"; i++; }
                else if (curr == 'ち' && next == 'ょ') { finalName += "cho"; i++; }
                else if (curr == 'ち') finalName += "chi";
                else if (curr == 'つ') finalName += "tsu";
                else if (curr == 'て') finalName += "te";
                else if (curr == 'と') finalName += "to";
                else if (curr == 'だ') finalName += "da";
                else if (curr == 'ぢ' && next == 'ゃ') { finalName += "dja"; i++; }
                else if (curr == 'ぢ' && next == 'ぃ') { finalName += "dji"; i++; }
                else if (curr == 'ぢ' && next == 'ゅ') { finalName += "dju"; i++; }
                else if (curr == 'ぢ' && next == 'ぇ') { finalName += "dje"; i++; }
                else if (curr == 'ぢ' && next == 'ょ') { finalName += "djo"; i++; }
                else if (curr == 'ぢ') finalName += "dji";
                else if (curr == 'づ') finalName += "dzu";
                else if (curr == 'で') finalName += "de";
                else if (curr == 'ど') finalName += "do";
                else if (curr == 'な') finalName += "na";
                else if (curr == 'に' && next == 'ゃ') { finalName += "nya"; i++; }
                else if (curr == 'に' && next == 'ぃ') { finalName += "nyi"; i++; }
                else if (curr == 'に' && next == 'ゅ') { finalName += "nyu"; i++; }
                else if (curr == 'に' && next == 'ぇ') { finalName += "nye"; i++; }
                else if (curr == 'に' && next == 'ょ') { finalName += "nyo"; i++; }
                else if (curr == 'に') finalName += "ni";
                else if (curr == 'ぬ') finalName += "nu";
                else if (curr == 'ね') finalName += "ne";
                else if (curr == 'の') finalName += "no";
                else if (curr == 'は') finalName += "ha";
                else if (curr == 'ひ' && next == 'ゃ') { finalName += "hya"; i++; }
                else if (curr == 'ひ' && next == 'ぃ') { finalName += "hyi"; i++; }
                else if (curr == 'ひ' && next == 'ゅ') { finalName += "hyu"; i++; }
                else if (curr == 'ひ' && next == 'ぇ') { finalName += "hye"; i++; }
                else if (curr == 'ひ' && next == 'ょ') { finalName += "hyo"; i++; }
                else if (curr == 'ひ') finalName += "hi";
                else if (curr == 'ふ') finalName += "fu";
                else if (curr == 'へ') finalName += "he";
                else if (curr == 'ほ') finalName += "ho";
                else if (curr == 'ば') finalName += "ba";
                else if (curr == 'び' && next == 'ゃ') { finalName += "bya"; i++; }
                else if (curr == 'び' && next == 'ぃ') { finalName += "byi"; i++; }
                else if (curr == 'び' && next == 'ゅ') { finalName += "byu"; i++; }
                else if (curr == 'び' && next == 'ぇ') { finalName += "bye"; i++; }
                else if (curr == 'び' && next == 'ょ') { finalName += "byo"; i++; }
                else if (curr == 'び') finalName += "bi";
                else if (curr == 'ぶ') finalName += "bu";
                else if (curr == 'べ') finalName += "be";
                else if (curr == 'ぼ') finalName += "bo";
                else if (curr == 'ぱ') finalName += "pa";
                else if (curr == 'ぴ' && next == 'ゃ') { finalName += "pya"; i++; }
                else if (curr == 'ぴ' && next == 'ぃ') { finalName += "pyi"; i++; }
                else if (curr == 'ぴ' && next == 'ゅ') { finalName += "pyu"; i++; }
                else if (curr == 'ぴ' && next == 'ぇ') { finalName += "pye"; i++; }
                else if (curr == 'ぴ' && next == 'ょ') { finalName += "pyo"; i++; }
                else if (curr == 'ぴ') finalName += "pi";
                else if (curr == 'ぷ') finalName += "pu";
                else if (curr == 'ぺ') finalName += "pe";
                else if (curr == 'ぽ') finalName += "po";
                else if (curr == 'ま') finalName += "ma";
                else if (curr == 'み' && next == 'ゃ') { finalName += "mya"; i++; }
                else if (curr == 'み' && next == 'ぃ') { finalName += "myi"; i++; }
                else if (curr == 'み' && next == 'ゅ') { finalName += "myu"; i++; }
                else if (curr == 'み' && next == 'ぇ') { finalName += "mye"; i++; }
                else if (curr == 'み' && next == 'ょ') { finalName += "myo"; i++; }
                else if (curr == 'み') finalName += "mi";
                else if (curr == 'む') finalName += "mu";
                else if (curr == 'め') finalName += "me";
                else if (curr == 'も') finalName += "mo";
                else if (curr == 'や') finalName += "ya";
                else if (curr == 'い' && next == 'ぇ') finalName += "ye";
                else if (curr == 'ゆ') finalName += "yu";
                else if (curr == 'よ') finalName += "yo";
                else if (curr == 'ら') finalName += "ra";
                else if (curr == 'り' && next == 'ゃ') { finalName += "rya"; i++; }
                else if (curr == 'り' && next == 'ぃ') { finalName += "ryi"; i++; }
                else if (curr == 'り' && next == 'ゅ') { finalName += "ryu"; i++; }
                else if (curr == 'り' && next == 'ぇ') { finalName += "rye"; i++; }
                else if (curr == 'り' && next == 'ょ') { finalName += "ryo"; i++; }
                else if (curr == 'り') finalName += "ri";
                else if (curr == 'る') finalName += "ru";
                else if (curr == 'れ') finalName += "re";
                else if (curr == 'ろ') finalName += "ro";
                else if (curr == 'わ') finalName += "wa";
                else if (curr == 'ゐ') finalName += "wi";
                else if (curr == 'ゑ') finalName += "we";
                else if (curr == 'を') finalName += "wo";
                else if (curr == 'ゔ') finalName += "vu";
                else if (curr == 'ん') finalName += "n";
                else finalName += curr;
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
            for (int i = 0; i < name.Length; i++)
            {
                finalName = "";
                doubleVoy--;
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                char nnext = ((i < name.Length - 2) ? (name[i + 2]) : (' '));
                if (curr == 'ッ')
                {
                    doubleVoy = 2;
                }
                else if (curr == 'ア') finalName += 'a';
                else if (curr == 'イ') finalName += 'i';
                else if (curr == 'ウ') finalName += 'u';
                else if (curr == 'エ') finalName += 'e';
                else if (curr == 'オ') finalName += 'o';
                else if (curr == 'カ') finalName += "ka";
                else if (curr == 'キ' && next == 'ャ') { finalName += "kya"; i++; }
                else if (curr == 'キ' && next == 'ィ') { finalName += "kyi"; i++; }
                else if (curr == 'キ' && next == 'ュ') { finalName += "kyu"; i++; }
                else if (curr == 'キ' && next == 'ェ') { finalName += "kye"; i++; }
                else if (curr == 'キ' && next == 'ョ') { finalName += "kyo"; i++; }
                else if (curr == 'キ') finalName += "ki";
                else if (curr == 'ク') finalName += "ku";
                else if (curr == 'ケ') finalName += "ke";
                else if (curr == 'コ') finalName += "ko";
                else if (curr == 'ガ') finalName += "ga";
                else if (curr == 'ギ' && next == 'ャ') { finalName += "gya"; i++; }
                else if (curr == 'ギ' && next == 'ィ') { finalName += "gyi"; i++; }
                else if (curr == 'ギ' && next == 'ュ') { finalName += "gyu"; i++; }
                else if (curr == 'ギ' && next == 'ェ') { finalName += "gye"; i++; }
                else if (curr == 'ギ' && next == 'ョ') { finalName += "gyo"; i++; }
                else if (curr == 'ギ') finalName += "gi";
                else if (curr == 'グ') finalName += "gu";
                else if (curr == 'ゲ') finalName += "ge";
                else if (curr == 'ゴ') finalName += "go";
                else if (curr == 'サ') finalName += "sa";
                else if (curr == 'シ' && next == 'ャ') { finalName += "sha"; i++; }
                else if (curr == 'シ' && next == 'ュ') { finalName += "shu"; i++; }
                else if (curr == 'シ' && next == 'ェ') { finalName += "she"; i++; }
                else if (curr == 'シ' && next == 'ョ') { finalName += "sho"; i++; }
                else if (curr == 'シ') finalName += "shi";
                else if (curr == 'ス') finalName += "su";
                else if (curr == 'セ') finalName += "se";
                else if (curr == 'ソ') finalName += "so";
                else if (curr == 'ザ') finalName += "za";
                else if (curr == 'ジ' && next == 'ャ') { finalName += "dja"; i++; }
                else if (curr == 'ジ' && next == 'ィ') { finalName += "dji"; i++; }
                else if (curr == 'ジ' && next == 'ュ') { finalName += "dju"; i++; }
                else if (curr == 'ジ' && next == 'ェ') { finalName += "dje"; i++; }
                else if (curr == 'ジ' && next == 'ョ') { finalName += "djo"; i++; }
                else if (curr == 'ジ') finalName += "dji";
                else if (curr == 'ズ') finalName += "zu";
                else if (curr == 'ゼ') finalName += "ze";
                else if (curr == 'ゾ') finalName += "zo";
                else if (curr == 'タ') finalName += "ta";
                else if (curr == 'チ' && next == 'ャ') { finalName += "cha"; i++; }
                else if (curr == 'チ' && next == 'ィ') { finalName += "chi"; i++; }
                else if (curr == 'チ' && next == 'ュ') { finalName += "chu"; i++; }
                else if (curr == 'チ' && next == 'ェ') { finalName += "che"; i++; }
                else if (curr == 'チ' && next == 'ョ') { finalName += "cho"; i++; }
                else if (curr == 'チ') finalName += "chi";
                else if (curr == 'ツ') finalName += "tsu";
                else if (curr == 'テ') finalName += "te";
                else if (curr == 'ト') finalName += "to";
                else if (curr == 'ダ') finalName += "da";
                else if (curr == 'ジ' && next == 'ャ') { finalName += "dja"; i++; }
                else if (curr == 'ジ' && next == 'ィ') { finalName += "dji"; i++; }
                else if (curr == 'ジ' && next == 'ュ') { finalName += "dju"; i++; }
                else if (curr == 'ジ' && next == 'ェ') { finalName += "dje"; i++; }
                else if (curr == 'ジ' && next == 'ョ') { finalName += "djo"; i++; }
                else if (curr == 'ジ') finalName += "dji";
                else if (curr == 'ヅ') finalName += "dzu";
                else if (curr == 'デ') finalName += "de";
                else if (curr == 'ド') finalName += "do";
                else if (curr == 'ナ') finalName += "na";
                else if (curr == 'ニ' && next == 'ャ') { finalName += "nya"; i++; }
                else if (curr == 'ニ' && next == 'ィ') { finalName += "nyi"; i++; }
                else if (curr == 'ニ' && next == 'ュ') { finalName += "nyu"; i++; }
                else if (curr == 'ニ' && next == 'ェ') { finalName += "nye"; i++; }
                else if (curr == 'ニ' && next == 'ョ') { finalName += "nyo"; i++; }
                else if (curr == 'ニ') finalName += "ni";
                else if (curr == 'ヌ') finalName += "nu";
                else if (curr == 'ネ') finalName += "ne";
                else if (curr == 'ノ') finalName += "no";
                else if (curr == 'ハ') finalName += "ha";
                else if (curr == 'ヒ' && next == 'ャ') { finalName += "hya"; i++; }
                else if (curr == 'ヒ' && next == 'ィ') { finalName += "hyi"; i++; }
                else if (curr == 'ヒ' && next == 'ュ') { finalName += "hyu"; i++; }
                else if (curr == 'ヒ' && next == 'ェ') { finalName += "hye"; i++; }
                else if (curr == 'ヒ' && next == 'ョ') { finalName += "hyo"; i++; }
                else if (curr == 'ヒ') finalName += "hi";
                else if (curr == 'フ') finalName += "fu";
                else if (curr == 'ヘ') finalName += "he";
                else if (curr == 'ホ') finalName += "ho";
                else if (curr == 'バ') finalName += "ba";
                else if (curr == 'ビ' && next == 'ャ') { finalName += "bya"; i++; }
                else if (curr == 'ビ' && next == 'ィ') { finalName += "byi"; i++; }
                else if (curr == 'ビ' && next == 'ュ') { finalName += "byu"; i++; }
                else if (curr == 'ビ' && next == 'ェ') { finalName += "bye"; i++; }
                else if (curr == 'ビ' && next == 'ョ') { finalName += "byo"; i++; }
                else if (curr == 'ビ') finalName += "bi";
                else if (curr == 'ブ') finalName += "bu";
                else if (curr == 'ベ') finalName += "be";
                else if (curr == 'ボ') finalName += "bo";
                else if (curr == 'パ') finalName += "pa";
                else if (curr == 'ピ' && next == 'ャ') { finalName += "pya"; i++; }
                else if (curr == 'ピ' && next == 'ィ') { finalName += "pyi"; i++; }
                else if (curr == 'ピ' && next == 'ュ') { finalName += "pyu"; i++; }
                else if (curr == 'ピ' && next == 'ェ') { finalName += "pye"; i++; }
                else if (curr == 'ピ' && next == 'ョ') { finalName += "pyo"; i++; }
                else if (curr == 'ピ') finalName += "pi";
                else if (curr == 'プ') finalName += "pu";
                else if (curr == 'ペ') finalName += "pe";
                else if (curr == 'ポ') finalName += "po";
                else if (curr == 'マ') finalName += "ma";
                else if (curr == 'ミ' && next == 'ャ') { finalName += "mya"; i++; }
                else if (curr == 'ミ' && next == 'ィ') { finalName += "myi"; i++; }
                else if (curr == 'ミ' && next == 'ュ') { finalName += "myu"; i++; }
                else if (curr == 'ミ' && next == 'ェ') { finalName += "mye"; i++; }
                else if (curr == 'ミ' && next == 'ョ') { finalName += "myo"; i++; }
                else if (curr == 'ミ') finalName += "mi";
                else if (curr == 'ム') finalName += "mu";
                else if (curr == 'メ') finalName += "me";
                else if (curr == 'モ') finalName += "mo";
                else if (curr == 'ヤ') finalName += "ya";
                else if (curr == 'イ' && next == 'ェ') { finalName += "ye"; i++; }
                else if (curr == 'ユ') finalName += "yu";
                else if (curr == 'ヨ') finalName += "yo";
                else if (curr == 'ラ') finalName += "ra";
                else if (curr == 'リ' && next == 'ャ') { finalName += "rya"; i++; }
                else if (curr == 'リ' && next == 'ィ') { finalName += "ryi"; i++; }
                else if (curr == 'リ' && next == 'ュ') { finalName += "ryu"; i++; }
                else if (curr == 'リ' && next == 'ェ') { finalName += "rye"; i++; }
                else if (curr == 'リ' && next == 'ョ') { finalName += "ryo"; i++; }
                else if (curr == 'リ') finalName += "ri";
                else if (curr == 'ル') finalName += "ru";
                else if (curr == 'レ') finalName += "re";
                else if (curr == 'ロ') finalName += "ro";
                else if (curr == 'ワ') finalName += "wa";
                else if (curr == 'ウ' && next == 'ィ') { finalName += "wi"; i++; }
                else if (curr == 'ウ' && next == 'ェ') { finalName += "we"; i++; }
                else if (curr == 'ウ') finalName += "wu";
                else if (curr == 'ヲ') finalName += "wo";
                else if (curr == 'ヴ' && next == 'ャ') { finalName += "va"; i++; }
                else if (curr == 'ヴ' && next == 'ィ') { finalName += "vi"; i++; }
                else if (curr == 'ヴ' && next == 'ェ') { finalName += "ve"; i++; }
                else if (curr == 'ヴ' && next == 'ォ') { finalName += "vo"; i++; }
                else if (curr == 'ヴ') finalName += "vu";
                else if (curr == 'ン') finalName += "n";
                else if (curr == 'ー' && finalStr.Length > 0) finalName += finalStr.Substring(finalStr.Length - 1, 1);
                else finalName += curr;
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

        public static string toHiragana(string name)
        {
            string finalName = "";
            name = name.ToLower();
            for (int i = 0; i < name.Length; i += 2)
            {
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                char nnext = ((i < name.Length - 2) ? (name[i + 2]) : (' '));
                if (curr != 'a' && curr != 'i' && curr != 'u' && curr != 'e' && curr != 'o' && curr != 'n'
                    && curr == next) { finalName += "っ"; i--; continue; }
                if (curr == 'a') { finalName += 'あ'; i--; }
                else if (curr == 'i') { finalName += 'い'; i--; }
                else if (curr == 'u') { finalName += 'う'; i--; }
                else if (curr == 'e') { finalName += 'え'; i--; }
                else if (curr == 'o') { finalName += 'お'; i--; }
                else if (curr == 'k' && next == 'a') finalName += "か";
                else if (curr == 'k' && next == 'y' && nnext == 'a') { finalName += "きゃ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'i') { finalName += "きぃ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'u') { finalName += "きゅ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'e') { finalName += "きぇ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'o') { finalName += "きょ"; i++; }
                else if (curr == 'k' && next == 'i') finalName += "き";
                else if (curr == 'k' && next == 'u') finalName += "く";
                else if (curr == 'k' && next == 'e') finalName += "け";
                else if (curr == 'k' && next == 'o') finalName += "こ";
                else if (curr == 'g' && next == 'a') finalName += "が";
                else if (curr == 'g' && next == 'y' && nnext == 'a') { finalName += "ぎゃ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'i') { finalName += "ぎぃ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'u') { finalName += "ぎゅ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'e') { finalName += "ぎぇ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'o') { finalName += "ぎょ"; i++; }
                else if (curr == 'g' && next == 'i') finalName += "ぎ";
                else if (curr == 'g' && next == 'u') finalName += "ぐ";
                else if (curr == 'g' && next == 'e') finalName += "げ";
                else if (curr == 'g' && next == 'o') finalName += "ご";
                else if (curr == 's' && next == 'a') finalName += "さ";
                else if (curr == 's' && next == 'h' && nnext == 'i') { finalName += "し"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'a') { finalName += "しゃ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'u') { finalName += "しゅ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'e') { finalName += "しぇ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'o') { finalName += "しょ"; i++; }
                else if (curr == 's' && next == 'u') finalName += "す";
                else if (curr == 's' && next == 'e') finalName += "せ";
                else if (curr == 's' && next == 'o') finalName += "そ";
                else if (curr == 'z' && next == 'a') finalName += "ざ";
                else if (curr == 'j' && next == 'i') finalName += "じ";
                else if (curr == 'j' && next == 'y' && nnext == 'a') { finalName += "じゃ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'i') { finalName += "じぃ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'u') { finalName += "じゅ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'e') { finalName += "じぇ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'o') { finalName += "じょ"; i++; }
                else if (curr == 'j' && next == 'a') finalName += "じゃ";
                else if (curr == 'j' && next == 'u') finalName += "じゅ";
                else if (curr == 'j' && next == 'e') finalName += "じぇ";
                else if (curr == 'j' && next == 'o') finalName += "じょ";
                else if (curr == 'z' && next == 'u') finalName += "ず";
                else if (curr == 'z' && next == 'e') finalName += "ぜ";
                else if (curr == 'z' && next == 'o') finalName += "ぞ";
                else if (curr == 't' && next == 'a') finalName += "た";
                else if (curr == 't' && next == 'y' && nnext == 'a') { finalName += "ちゃ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'i') { finalName += "ちぃ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'u') { finalName += "ちゅ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'e') { finalName += "ちぇ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'o') { finalName += "ちょ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'i') { finalName += "ち"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'a') { finalName += "ちゃ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'u') { finalName += "ちゅ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'e') { finalName += "ちぇ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'o') { finalName += "ちょ"; i++; }
                else if (curr == 't' && next == 's' && nnext == 'u') { finalName += "つ"; i++; }
                else if (curr == 't' && next == 'e') finalName += "て";
                else if (curr == 't' && next == 'o') finalName += "と";
                else if (curr == 'd' && next == 'a') finalName += "だ";
                else if (curr == 'd' && next == 'y' && nnext == 'a') { finalName += "ぢゃ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'i') { finalName += "ぢぃ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'u') { finalName += "ぢゅ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'e') { finalName += "ぢぇ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'o') { finalName += "ぢょ"; i++; }
                else if (curr == 'd' && next == 'z' && next == 'u') { finalName += "づ"; i++; }
                else if (curr == 'z' && next == 'u') finalName += "づ";
                else if (curr == 'd' && next == 'e') finalName += "で";
                else if (curr == 'd' && next == 'o') finalName += "ど";
                else if (curr == 'n' && next == 'a') finalName += "な";
                else if (curr == 'n' && next == 'y' && nnext == 'a') { finalName += "にゃ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'i') { finalName += "にぃ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'u') { finalName += "にゅ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'e') { finalName += "にぇ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'o') { finalName += "にょ"; i++; }
                else if (curr == 'n' && next == 'i') finalName += "に";
                else if (curr == 'n' && next == 'u') finalName += "ぬ";
                else if (curr == 'n' && next == 'e') finalName += "ね";
                else if (curr == 'n' && next == 'o') finalName += "の";
                else if (curr == 'h' && next == 'a') finalName += "は";
                else if (curr == 'h' && next == 'y' && nnext == 'a') { finalName += "ひゃ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'i') { finalName += "ひぃ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'u') { finalName += "ひゅ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'e') { finalName += "ひぇ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'o') { finalName += "ひょ"; i++; }
                else if (curr == 'h' && next == 'i') finalName += "ひ";
                else if (curr == 'f' && next == 'u') finalName += "ふ";
                else if (curr == 'h' && next == 'e') finalName += "へ";
                else if (curr == 'h' && next == 'o') finalName += "ほ";
                else if (curr == 'b' && next == 'a') finalName += "ば";
                else if (curr == 'b' && next == 'y' && nnext == 'a') { finalName += "びゃ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'i') { finalName += "びぃ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'u') { finalName += "びゅ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'e') { finalName += "びぇ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'o') { finalName += "びょ"; i++; }
                else if (curr == 'b' && next == 'i') finalName += "び";
                else if (curr == 'b' && next == 'u') finalName += "ぶ";
                else if (curr == 'b' && next == 'e') finalName += "べ";
                else if (curr == 'b' && next == 'o') finalName += "ぼ";
                else if (curr == 'p' && next == 'a') finalName += "ぱ";
                else if (curr == 'p' && next == 'y' && nnext == 'a') { finalName += "ぴゃ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'i') { finalName += "ぴぃ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'u') { finalName += "ぴゅ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'e') { finalName += "ぴぇ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'o') { finalName += "ぴょ"; i++; }
                else if (curr == 'p' && next == 'i') finalName += "ぴ";
                else if (curr == 'p' && next == 'u') finalName += "ぷ";
                else if (curr == 'p' && next == 'e') finalName += "ぺ";
                else if (curr == 'p' && next == 'o') finalName += "ぽ";
                else if (curr == 'm' && next == 'a') finalName += "ま";
                else if (curr == 'm' && next == 'y' && nnext == 'a') { finalName += "みゃ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'i') { finalName += "みぃ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'u') { finalName += "みゅ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'e') { finalName += "みぇ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'o') { finalName += "みょ"; i++; }
                else if (curr == 'm' && next == 'i') finalName += "み";
                else if (curr == 'm' && next == 'u') finalName += "む";
                else if (curr == 'm' && next == 'e') finalName += "め";
                else if (curr == 'm' && next == 'o') finalName += "も";
                else if (curr == 'y' && next == 'a') finalName += "や";
                else if (curr == 'y' && next == 'u') finalName += "ゆ";
                else if (curr == 'y' && next == 'o') finalName += "よ";
                else if (curr == 'r' && next == 'a') finalName += "ら";
                else if (curr == 'r' && next == 'y' && nnext == 'a') { finalName += "りゃ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'i') { finalName += "りぃ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'u') { finalName += "りゅ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'e') { finalName += "りぇ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'o') { finalName += "りょ"; i++; }
                else if (curr == 'r' && next == 'i') finalName += "り";
                else if (curr == 'r' && next == 'u') finalName += "る";
                else if (curr == 'r' && next == 'e') finalName += "れ";
                else if (curr == 'r' && next == 'o') finalName += "ろ";
                else if (curr == 'w' && next == 'a') finalName += "わ";
                else if (curr == 'w' && next == 'i') finalName += "ゐ";
                else if (curr == 'w' && next == 'e') finalName += "ゑ";
                else if (curr == 'w' && next == 'o') finalName += "を";
                else if (curr == 'v' && next == 'u') finalName += "ゔ";
                else if (curr == 'n') { finalName += "ん"; i--; }
                else { finalName += curr; i--; }
            }
            return (finalName);
        }

        public static string toKatakana(string name)
        {
            string finalName = "";
            name = name.ToLower();
            for (int i = 0; i < name.Length; i += 2)
            {
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                char nnext = ((i < name.Length - 2) ? (name[i + 2]) : (' '));
                if (curr != 'a' && curr != 'i' && curr != 'u' && curr != 'e' && curr != 'o' && curr != 'n'
                    && curr == next)
                { finalName += "ッ"; i--; continue; }
                if (curr == 'a') { finalName += 'ア'; i--; }
                else if (curr == 'i') { finalName += 'イ'; i--; }
                else if (curr == 'u') { finalName += 'ウ'; i--; }
                else if (curr == 'e') { finalName += 'エ'; i--; }
                else if (curr == 'o') { finalName += 'オ'; i--; }
                else if (curr == 'k' && next == 'a') finalName += "か";
                else if (curr == 'k' && next == 'y' && nnext == 'a') { finalName += "キャ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'i') { finalName += "キィ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'u') { finalName += "キュ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'e') { finalName += "キェ"; i++; }
                else if (curr == 'k' && next == 'y' && nnext == 'o') { finalName += "キョ"; i++; }
                else if (curr == 'k' && next == 'i') finalName += "キ";
                else if (curr == 'k' && next == 'u') finalName += "ク";
                else if (curr == 'k' && next == 'e') finalName += "ケ";
                else if (curr == 'k' && next == 'o') finalName += "コ";
                else if (curr == 'g' && next == 'a') finalName += "ガ";
                else if (curr == 'g' && next == 'y' && nnext == 'a') { finalName += "ギャ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'i') { finalName += "ギィ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'u') { finalName += "ギュ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'e') { finalName += "ギェ"; i++; }
                else if (curr == 'g' && next == 'y' && nnext == 'o') { finalName += "ギョ"; i++; }
                else if (curr == 'g' && next == 'i') finalName += "ギ";
                else if (curr == 'g' && next == 'u') finalName += "グ";
                else if (curr == 'g' && next == 'e') finalName += "ゲ";
                else if (curr == 'g' && next == 'o') finalName += "ゴ";
                else if (curr == 's' && next == 'a') finalName += "サ";
                else if (curr == 's' && next == 'h' && nnext == 'i') { finalName += "シ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'a') { finalName += "シャ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'u') { finalName += "シュ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'e') { finalName += "シェ"; i++; }
                else if (curr == 's' && next == 'h' && nnext == 'o') { finalName += "ショ"; i++; }
                else if (curr == 's' && next == 'u') finalName += "ス";
                else if (curr == 's' && next == 'e') finalName += "セ";
                else if (curr == 's' && next == 'o') finalName += "ソ";
                else if (curr == 'z' && next == 'a') finalName += "ザ";
                else if (curr == 'j' && next == 'i') finalName += "ジ";
                else if (curr == 'j' && next == 'y' && nnext == 'a') { finalName += "ジャ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'i') { finalName += "ジィ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'u') { finalName += "ジュ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'e') { finalName += "ジェ"; i++; }
                else if (curr == 'j' && next == 'y' && nnext == 'o') { finalName += "ジョ"; i++; }
                else if (curr == 'j' && next == 'a') finalName += "ジャ";
                else if (curr == 'j' && next == 'u') finalName += "ジュ";
                else if (curr == 'j' && next == 'e') finalName += "ジェ";
                else if (curr == 'j' && next == 'o') finalName += "ジョ";
                else if (curr == 'z' && next == 'u') finalName += "ズ";
                else if (curr == 'z' && next == 'e') finalName += "ゼ";
                else if (curr == 'z' && next == 'o') finalName += "ゾ";
                else if (curr == 't' && next == 'a') finalName += "タ";
                else if (curr == 't' && next == 'y' && nnext == 'a') { finalName += "チャ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'i') { finalName += "チィ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'u') { finalName += "チュ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'e') { finalName += "チェ"; i++; }
                else if (curr == 't' && next == 'y' && nnext == 'o') { finalName += "チョ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'i') { finalName += "チ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'a') { finalName += "チャ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'u') { finalName += "チュ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'e') { finalName += "チェ"; i++; }
                else if (curr == 'c' && next == 'h' && nnext == 'o') { finalName += "チョ"; i++; }
                else if (curr == 't' && next == 's' && nnext == 'u') { finalName += "ツ"; i++; }
                else if (curr == 't' && next == 'e') finalName += "テ";
                else if (curr == 't' && next == 'o') finalName += "ト";
                else if (curr == 'd' && next == 'a') finalName += "ダ";
                else if (curr == 'd' && next == 'y' && nnext == 'a') { finalName += "ヂャ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'i') { finalName += "ヂィ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'u') { finalName += "ヂュ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'e') { finalName += "ヂェ"; i++; }
                else if (curr == 'd' && next == 'y' && nnext == 'o') { finalName += "ヂョ"; i++; }
                else if (curr == 'd' && next == 'z' && next == 'u') { finalName += "ヅ"; i++; }
                else if (curr == 'z' && next == 'u') finalName += "ズ";
                else if (curr == 'd' && next == 'e') finalName += "デ";
                else if (curr == 'd' && next == 'o') finalName += "ド";
                else if (curr == 'n' && next == 'a') finalName += "ナ";
                else if (curr == 'n' && next == 'y' && nnext == 'a') { finalName += "ニャ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'i') { finalName += "ニィ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'u') { finalName += "ニュ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'e') { finalName += "ニェ"; i++; }
                else if (curr == 'n' && next == 'y' && nnext == 'o') { finalName += "ニョ"; i++; }
                else if (curr == 'n' && next == 'i') finalName += "ニ";
                else if (curr == 'n' && next == 'u') finalName += "ヌ";
                else if (curr == 'n' && next == 'e') finalName += "ネ";
                else if (curr == 'n' && next == 'o') finalName += "ノ";
                else if (curr == 'h' && next == 'a') finalName += "ハ";
                else if (curr == 'h' && next == 'y' && nnext == 'a') { finalName += "ヒャ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'i') { finalName += "ヒィ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'u') { finalName += "ヒュ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'e') { finalName += "ヒェ"; i++; }
                else if (curr == 'h' && next == 'y' && nnext == 'o') { finalName += "ヒョ"; i++; }
                else if (curr == 'h' && next == 'i') finalName += "ヒ";
                else if (curr == 'f' && next == 'u') finalName += "フ";
                else if (curr == 'h' && next == 'e') finalName += "ヘ";
                else if (curr == 'h' && next == 'o') finalName += "ホ";
                else if (curr == 'b' && next == 'a') finalName += "バ";
                else if (curr == 'b' && next == 'y' && nnext == 'a') { finalName += "ビャ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'i') { finalName += "ビィ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'u') { finalName += "ビュ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'e') { finalName += "ビェ"; i++; }
                else if (curr == 'b' && next == 'y' && nnext == 'o') { finalName += "ビョ"; i++; }
                else if (curr == 'b' && next == 'i') finalName += "ビ";
                else if (curr == 'b' && next == 'u') finalName += "ブ";
                else if (curr == 'b' && next == 'e') finalName += "ベ";
                else if (curr == 'b' && next == 'o') finalName += "ボ";
                else if (curr == 'p' && next == 'a') finalName += "パ";
                else if (curr == 'p' && next == 'y' && nnext == 'a') { finalName += "ピャ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'i') { finalName += "ピィ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'u') { finalName += "ピュ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'e') { finalName += "ピェ"; i++; }
                else if (curr == 'p' && next == 'y' && nnext == 'o') { finalName += "ピョ"; i++; }
                else if (curr == 'p' && next == 'i') finalName += "ピ";
                else if (curr == 'p' && next == 'u') finalName += "プ";
                else if (curr == 'p' && next == 'e') finalName += "ペ";
                else if (curr == 'p' && next == 'o') finalName += "ポ";
                else if (curr == 'm' && next == 'a') finalName += "マ";
                else if (curr == 'm' && next == 'y' && nnext == 'a') { finalName += "ミャ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'i') { finalName += "ミィ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'u') { finalName += "ミュ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'e') { finalName += "ミェ"; i++; }
                else if (curr == 'm' && next == 'y' && nnext == 'o') { finalName += "ミョ"; i++; }
                else if (curr == 'm' && next == 'i') finalName += "ミ";
                else if (curr == 'm' && next == 'u') finalName += "ム";
                else if (curr == 'm' && next == 'e') finalName += "メ";
                else if (curr == 'm' && next == 'o') finalName += "モ";
                else if (curr == 'y' && next == 'a') finalName += "ヤ";
                else if (curr == 'y' && next == 'u') finalName += "ユ";
                else if (curr == 'y' && next == 'o') finalName += "ヨ";
                else if (curr == 'r' && next == 'a') finalName += "ラ";
                else if (curr == 'r' && next == 'y' && nnext == 'a') { finalName += "リャ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'i') { finalName += "リィ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'u') { finalName += "リュ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'e') { finalName += "リェ"; i++; }
                else if (curr == 'r' && next == 'y' && nnext == 'o') { finalName += "リョ"; i++; }
                else if (curr == 'r' && next == 'i') finalName += "リ";
                else if (curr == 'r' && next == 'u') finalName += "ル";
                else if (curr == 'r' && next == 'e') finalName += "レ";
                else if (curr == 'r' && next == 'o') finalName += "ロ";
                else if (curr == 'w' && next == 'a') finalName += "ワ";
                else if (curr == 'w' && next == 'i') finalName += "ウィ";
                else if (curr == 'w' && next == 'e') finalName += "ウェ";
                else if (curr == 'w' && next == 'u') finalName += "ウ";
                else if (curr == 'w' && next == 'o') finalName += "ヲ";
                else if (curr == 'v' && next == 'a') finalName += "ヴャ";
                else if (curr == 'v' && next == 'i') finalName += "ヴィ";
                else if (curr == 'v' && next == 'e') finalName += "ヴェ";
                else if (curr == 'v' && next == 'o') finalName += "ヴォ";
                else if (curr == 'v' && next == 'u') finalName += "ヴ";
                else if (curr == 'n') { finalName += "ン"; i--; }
                else { finalName += curr; i--; }
            }
            return (finalName);
        }
    }
}