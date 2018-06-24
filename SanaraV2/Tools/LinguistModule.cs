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
using SanaraV2.Tools.LinguistResources;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Google;
using Google.Cloud.Translation.V2;
using Google.Cloud.Vision.V1;
using Grpc.Core;
using System.Resources;
using SanaraV2.Base;

namespace SanaraV2.Tools
{
    public class LinguistModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Hiragana"), Summary("To hiragana")]
        public async Task ToHiraganaCmd(params string[] word)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.ToHiraganaHelp(Context.Guild.Id));
            else
                await ReplyAsync(ToHiragana(FromKatakana(Utilities.AddArgs(word))));
        }

        [Command("Romaji"), Summary("To romaji")]
        public async Task ToRomajiCmd(params string[] word)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.ToRomajiHelp(Context.Guild.Id));
            else
                await ReplyAsync(FromKatakana(FromHiragana(Utilities.AddArgs(word))));
        }

        [Command("Katakana"), Summary("To romaji")]
        public async Task ToKatakanaCmd(params string[] word)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.ToKatakanaHelp(Context.Guild.Id));
            else
                await ReplyAsync(ToKatakana(FromHiragana(Utilities.AddArgs(word))));
        }

        [Command("Translation", RunMode = RunMode.Async), Summary("Translate a sentence")]
        public async Task Translation(params string[] words)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (p.translationClient == null)
                await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
            else if (words.Length < 2)
                await ReplyAsync(Sentences.TranslateHelp(Context.Guild.Id));
            else
            {
                string language;
                if (words[0].Length == 2)
                    language = words[0];
                else
                    language = Utilities.GetLanguage(words[0].ToLower());
                if (language == null)
                {
                    await ReplyAsync(Sentences.InvalidLanguage(Context.Guild.Id));
                    return;
                }
                List<string> newWords = words.ToList();
                newWords.RemoveAt(0);
                try
                {
                    string sourceLanguage;
                    string translation;
                    if (newWords.Count == 1 && Utilities.GetExtensionImage(newWords[0]) != null && Utilities.IsLinkValid(newWords[0]))
                    {
                        if (p.visionClient == null)
                        {
                            await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
                            return;
                        }
                        while (true)
                        {
                            try
                            {
                                Image image = await Image.FetchFromUriAsync(newWords[0]);
                                TextAnnotation response = await p.visionClient.DetectDocumentTextAsync(image);
                                translation = GetTranslation(response.Text, language, out sourceLanguage);
                                break;
                            }
                            catch (RpcException)
                            { }
                        }
                    }
                    else
                        translation = GetTranslation(Utilities.AddArgs(newWords.ToArray()), language, out sourceLanguage);
                    sourceLanguage = Utilities.GetFullLanguage(sourceLanguage.ToLower());
                    await ReplyAsync("From " + sourceLanguage + ":" + Environment.NewLine + "```" + Environment.NewLine + translation + Environment.NewLine + "```");
                }
                catch (GoogleApiException)
                {
                    await ReplyAsync(Sentences.InvalidLanguage(Context.Guild.Id));
                    return;
                }
            }
        }

        [Command("Definition", RunMode = RunMode.Async), Summary("Give the meaning of a word")]
        public async Task Meaning(params string[] word)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Linguistic);
            if (word.Length == 0)
                await ReplyAsync(Sentences.JapaneseHelp(Context.Guild.Id));
            else
                foreach (string s in GetAllKanjis(Utilities.AddArgs(word), Context.Guild.Id))
                    await ReplyAsync(s);
        }

        public static string GetTranslation(string words, string language, out string sourceLanguage)
        {
            TranslationResult translation = Program.p.translationClient.TranslateText(words, language);
            sourceLanguage = translation.DetectedSourceLanguage;
            return (translation.TranslatedText);
        }

        public static List<string> GetAllKanjis(string word, ulong guildId)
        {
            string newWord = word.Replace(" ", "%20");
            string json;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                json = wc.DownloadString("http://www.jisho.org/api/v1/search/words?keyword=" + newWord.ToLower());
            }
            string[] url = Utilities.GetElementXml("\"japanese\":[", json, '$').Split(new string[] { "\"japanese\":[" }, StringSplitOptions.None);
            string finalStr = Sentences.GiveJapaneseTranslations(guildId, word) + Environment.NewLine + Environment.NewLine;
            if (url[0] == "")
                return new List<string>() { Sentences.NoJapaneseTranslation(guildId, word) + "." };
            else
            {
                List<string> finalList = new List<string>();
                int counter = 0;
                foreach (string str in url)
                {
                    string[] urlResult = str.Split(new string[] { "},{" }, StringSplitOptions.None);
                    string[] meanings = Utilities.GetElementXml("english_definitions\":[", str, ']').Split(new string[] { "\",\"" }, StringSplitOptions.None);
                    foreach (string s in urlResult)
                    {
                        if (Utilities.GetElementXml("\"reading\":\"", s, '"') == "" && Utilities.GetElementXml("\"word\":\"", s, '"') == "")
                            continue;
                        finalStr += ((Utilities.GetElementXml("\"word\":\"", s, '"') == "") ? (Utilities.GetElementXml("\"reading\":\"", s, '"') + " (" + FromKatakana(FromHiragana(Utilities.GetElementXml("\"reading\":\"", s, '"') + ")")))
                        : (Utilities.GetElementXml("\"word\":\"", s, '"')
                        + ((Utilities.GetElementXml("\"reading\":\"", s, '"') == "") ? ("") : (" (" + Utilities.GetElementXml("\"reading\":\"", s, '"') + " - " + FromHiragana(FromKatakana(Utilities.GetElementXml("\"reading\":\"", s, '"') + ")")))))) + Environment.NewLine;
                    }
                    finalStr += Sentences.Meaning(guildId);
                    string allMeanings = String.Join(" / ", meanings);
                    finalStr += allMeanings.Substring(1, allMeanings.Length - 2);
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

        private static string Transcript(char curr, char next, ref int i, ResourceManager transcriptionArray)
        {
            if (next != ' ' && transcriptionArray.GetString("" + curr + next) != null)
            {
                i++;
                return (transcriptionArray.GetString("" + curr + next));
            }
            if (transcriptionArray.GetString("" + curr) != null)
                return (transcriptionArray.GetString("" + curr));
            return ("" + curr);
        }

        public static string FromHiragana(string name)
        {
            ResourceManager manager = HiraganaToRomaji.ResourceManager;
            string finalName = "";
            string finalStr = "";
            int doubleVoy = 0;
            for (int i = 0; i < name.Length; i++)
            {
                finalName = "";
                doubleVoy--;
                char curr = name[i];
                char next = ((i < name.Length - 1) ? (name[i + 1]) : (' '));
                if (curr == 'っ')
                    doubleVoy = 2;
                else
                    finalName += Transcript(curr, next, ref i, manager);
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

        public static string FromKatakana(string name)
        {
            ResourceManager manager = KatakanaToRomaji.ResourceManager;
            string finalName = "";
            string finalStr = "";
            int doubleVoy = 0;
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
                    finalName += Transcript(curr, next, ref i, manager);
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

        private static string TranscriptInvert(char curr, char next, char nnext, ref int i, ResourceManager transcriptionArray)
        {
            if (next != ' ')
            {
                if (nnext != ' ' && transcriptionArray.GetString("" + curr + next + nnext) != null)
                {
                    i++;
                    return (transcriptionArray.GetString("" + curr + next + nnext));
                }
                if (curr == 'd' && next == 'o')
                        return (transcriptionArray.GetString("_do"));
                if (transcriptionArray.GetString("" + curr + next) != null)
                    return (transcriptionArray.GetString("" + curr + next));
            }
            i--;
            if (transcriptionArray.GetString("" + curr) != null)
                return (transcriptionArray.GetString("" + curr));
            return ("" + curr);
        }

        public static string ToHiragana(string name)
        {
            ResourceManager manager = RomajiToHiragana.ResourceManager;
            string finalName = "";
            name = name.ToLower();
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
                    finalName += TranscriptInvert(curr, next, nnext, ref i, manager);
            }
            return (finalName);
        }

        public static string ToKatakana(string name)
        {
            ResourceManager manager = RomajiToKatakana.ResourceManager;
            string finalName = "";
            name = name.ToLower();
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
                else if ((next == 'a' || next == 'i' || next == 'u' || next == 'e' || next == 'o')
                   && next == nnext)
                {
                    finalName += TranscriptInvert(curr, next, nnext, ref i, manager) + "ー";
                    i++;
                }
                else
                    finalName += TranscriptInvert(curr, next, nnext, ref i, manager);
            }
            return (finalName);
        }
    }
}