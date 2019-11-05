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
using Google;
using Google.Cloud.Translation.V2;
using Google.Cloud.Vision.V1;
using Newtonsoft.Json;
using SanaraV2.Features.Tools.LinguistResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Features.Tools
{
    public static class Linguist
    {
        public static async Task<FeatureRequest<Response.Kanji, Error.Kanji>> Kanji(string[] args)
        {
            if (args.Length == 0)
                return new FeatureRequest<Response.Kanji, Error.Kanji>(null, Error.Kanji.Help);
            string argsEscape = Uri.EscapeDataString(string.Join(" ", args));
            string html;
            dynamic json;
            char kanji;
            string meaning;
            Match radicalMatch;
            Dictionary<string, string> parts = new Dictionary<string, string>();
            Dictionary<string, string> onyomi = new Dictionary<string, string>();
            Dictionary<string, string> kunyomi = new Dictionary<string, string>();
            using (HttpClient hc = new HttpClient())
            {
                json = JsonConvert.DeserializeObject(await hc.GetStringAsync("https://jisho.org/api/v1/search/words?keyword=" + argsEscape));
                if (json.data.Count == 0 || json.data[0].japanese[0].word == null)
                    return new FeatureRequest<Response.Kanji, Error.Kanji>(null, Error.Kanji.NotFound);
                kanji = ((string)json.data[0].japanese[0].word)[0];
                html = await hc.GetStringAsync("https://jisho.org/search/" + kanji + "%20%23kanji");
                radicalMatch = Regex.Match(html, "<span class=\"radical_meaning\">([^<]+)<\\/span>([^<]+)<\\/span>");
                meaning = Regex.Match(html, "<div class=\"kanji-details__main-meanings\">([^<]+)<\\/div>").Groups[1].Value.Trim();
                foreach (var match in Regex.Matches(html, "<a href=\"(\\/\\/jisho\\.org\\/search\\/[^k]+kanji)\">([^<]+)<\\/a>").Cast<Match>())
                {
                    string name = match.Groups[2].Value;
                    if (name[0] == kanji)
                        parts.Add(name, meaning);
                    else
                        parts.Add(name, Regex.Match(await hc.GetStringAsync("https:" + match.Groups[1].Value), "<div class=\"kanji-details__main-meanings\">([^<]+)<\\/div>").Groups[1].Value.Trim());
                }
                if (html.Contains("<dt>On:"))
                    foreach (var match in Regex.Matches(html.Split(new[] { "<dt>On:" }, StringSplitOptions.None)[1]
                        .Split(new[] { "</dd>" }, StringSplitOptions.None)[0],
                        "<a[^>]+>([^<]+)<\\/a>").Cast<Match>())
                        onyomi.Add(match.Groups[1].Value, ToRomaji(match.Groups[1].Value));
                if (html.Contains("<dt>Kun:"))
                    foreach (var match in Regex.Matches(html.Split(new[] { "<dt>Kun:" }, StringSplitOptions.None)[1]
                        .Split(new string[] { "</dd>" }, StringSplitOptions.None)[0],
                    "<a[^>]+>([^<]+)<\\/a>").Cast<Match>())
                        kunyomi.Add(match.Groups[1].Value, ToRomaji(match.Groups[1].Value));
            }
            return new FeatureRequest<Response.Kanji, Error.Kanji>(new Response.Kanji
            {
                kanji = kanji,
                meaning = meaning,
                strokeOrder = "http://classic.jisho.org/static/images/stroke_diagrams/" + (int)kanji + "_frames.png",
                parts = parts,
                radicalMeaning = radicalMatch.Groups[1].Value.Trim(),
                radicalKanji = radicalMatch.Groups[2].Value.Trim(),
                onyomi = onyomi,
                kunyomi = kunyomi
            }, Error.Kanji.None);
        }

        public static async Task<FeatureRequest<Response.Urban, Error.Urban>> UrbanSearch(bool isChanSafe, string[] args)
        {
            if (isChanSafe)
                return new FeatureRequest<Response.Urban, Error.Urban>(null, Error.Urban.ChanNotNSFW);
            if (args.Length == 0)
                return new FeatureRequest<Response.Urban, Error.Urban>(null, Error.Urban.Help);
            string searchUrl = "http://api.urbandictionary.com/v0/define?term=" + Utilities.AddArgs(args);
            string html;
            using (HttpClient hc = new HttpClient())
                html = await hc.GetStringAsync(searchUrl);
            dynamic json = JsonConvert.DeserializeObject(html);
            if (json.list.Count == 0)
                return new FeatureRequest<Response.Urban, Error.Urban>(null, Error.Urban.NotFound);
            string definition = json.list[0].definition;
            bool definitionComplete = true;
            while (definition.Length > 1015)
            {
                definitionComplete = false;
                string[] tmp = definition.Split('.');
                definition = string.Join(".", tmp.Take(tmp.Length - 1));
            }
            string example = ((string)json.list[0].example).Replace(Environment.NewLine, "\n");
            bool exampleComplete = true;
            while (example.Length > 1015)
            {
                exampleComplete = false;
                string[] tmp = example.Split(new string[] { "\n\n" }, StringSplitOptions.None);
                example = string.Join("\n\n", tmp.Take(tmp.Length - 1));
            }
            return new FeatureRequest<Response.Urban, Error.Urban>(new Response.Urban()
            {
                definition = definition + ((definitionComplete) ? ("") : (Environment.NewLine + Environment.NewLine + "[...]")),
                example = example + ((exampleComplete) ? ("") : (Environment.NewLine + Environment.NewLine + "[...]")),
                link = json.list[0].permalink,
                word = json.list[0].word
            }, Error.Urban.None);
        }

        public static async Task<FeatureRequest<Response.Translation, Error.Translation>> Translate(string[] args, TranslationClient translationClient,
            ImageAnnotatorClient visionClient, Dictionary<string, List<string>> allLanguages)
        {
            if (translationClient == null)
                return new FeatureRequest<Response.Translation, Error.Translation>(null, Error.Translation.InvalidApiKey);
            if (args.Length < 2)
                return new FeatureRequest<Response.Translation, Error.Translation>(null, Error.Translation.Help);
            string language;
            if (args[0].Length == 2)
                language = args[0];
            else
                language = Utilities.GetLanguage(args[0], allLanguages);
            if (language == null)
                return new FeatureRequest<Response.Translation, Error.Translation>(null, Error.Translation.InvalidLanguage);
            List<string> newWords = args.ToList();
            newWords.RemoveAt(0);
            try
            {
                string toTranslate = Utilities.AddArgs(newWords.ToArray());
                if (newWords.Count == 1 && Utilities.IsLinkValid(newWords[0]))
                {
                    if (visionClient == null)
                        return new FeatureRequest<Response.Translation, Error.Translation>(null, Error.Translation.InvalidApiKey);
                    Image image = await Image.FetchFromUriAsync(newWords[0]);
                    TextAnnotation response;
                    try
                    {
                        response = await visionClient.DetectDocumentTextAsync(image);
                    }
                    catch (AnnotateImageException)
                    {
                        return new FeatureRequest<Response.Translation, Error.Translation>(null, Error.Translation.NotAnImage);
                    }
                    if (response == null)
                        return new FeatureRequest<Response.Translation, Error.Translation>(null, Error.Translation.NoTextOnImage);
                    toTranslate = response.Text;
                }
                TranslationResult translation = await translationClient.TranslateTextAsync(toTranslate, language);
                return new FeatureRequest<Response.Translation, Error.Translation>(new Response.Translation()
                {
                    sentence = translation.TranslatedText,
                    sourceLanguage = Utilities.GetFullLanguage(translation.DetectedSourceLanguage, allLanguages)
                }, Error.Translation.None);
            }
            catch (GoogleApiException)
            {
                return new FeatureRequest<Response.Translation, Error.Translation>(null, Error.Translation.InvalidLanguage);
            }
        }

        public static async Task<FeatureRequest<Response.JapaneseTranslation[], Error.JapaneseTranslation>> JapaneseTranslate(string[] args)
        {
            if (args.Length == 0)
                return new FeatureRequest<Response.JapaneseTranslation[], Error.JapaneseTranslation>(null, Error.JapaneseTranslation.Help);
            dynamic json;
            using (HttpClient hc = new HttpClient())
                json = JsonConvert.DeserializeObject(await (await hc.GetAsync("http://www.jisho.org/api/v1/search/words?keyword=" + Utilities.AddArgs(args).ToLower())).Content.ReadAsStringAsync());
            if (json.data.Count == 0)
                return new FeatureRequest<Response.JapaneseTranslation[], Error.JapaneseTranslation>(null, Error.JapaneseTranslation.NotFound);
            List<Response.JapaneseTranslation> translations = new List<Response.JapaneseTranslation>();
            foreach (var data in json.data)
            {
                List<Response.JapaneseWord> words = new List<Response.JapaneseWord>();
                foreach (var wordData in data.japanese)
                    words.Add(new Response.JapaneseWord()
                    {
                        word = wordData.word,
                        reading = wordData.reading,
                        romaji = ToRomaji((string)wordData.reading)
                    });
                translations.Add(new Response.JapaneseTranslation()
                {
                    words = words.ToArray(),
                    definition = data.senses[0].english_definitions?.ToObject<string[]>(),
                    speechPart = data.parts_of_speech?.ToObject<string[]>()
                });
            }
            return new FeatureRequest<Response.JapaneseTranslation[], Error.JapaneseTranslation>(translations.ToArray(), Error.JapaneseTranslation.None);
        }

        public static string ToHiragana(string word)
            => word == null ? null : ToHiraganaInternal(FromKatakanaInternal(word));

        public static string ToRomaji(string word)
            => word == null ? null : FromKatakanaInternal(FromHiraganaInternal(word));

        public static string ToKatakana(string word)
            => word == null ? null : ToKatakanaInternal(FromHiraganaInternal(word));

        private static string TranscriptInternal(char curr, char next, ref int i, ResourceManager transcriptionArray)
        {
            if (next != ' ' && transcriptionArray.GetString("" + curr + next) != null)
            {
                i++;
                return transcriptionArray.GetString("" + curr + next);
            }
            if (transcriptionArray.GetString("" + curr) != null)
                return transcriptionArray.GetString("" + curr);
            return "" + curr;
        }

        private static string FromHiraganaInternal(string name)
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
                    finalName += TranscriptInternal(curr, next, ref i, manager);
                if (doubleVoy == 1 && curr != 'ん' && curr != 'ゔ' && curr != 'ゃ' && curr != 'ぃ' && curr != 'ゅ' && curr != 'ぇ' && curr != 'ょ'
                     && curr != 'っ' && curr != 'あ' && curr != 'い' && curr != 'う' && curr != 'え' && curr != 'お')
                {
                    finalName = finalName[0] + finalName;
                }
                finalStr += finalName;
                finalName = "";
            }
            finalStr += finalName;
            return finalStr;
        }

        private static char GetNextCharacter(char c)
        {
            if (c == 'o')
                return 'u';
            if (c == 'e')
                return 'i';
            return c;
        }

        private static string FromKatakanaInternal(string name)
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
                else if (curr == 'ー')
                    continue;
                else
                    finalName += TranscriptInternal(curr, next, ref i, manager);
                if (doubleVoy == 1 && curr != 'ン' && curr != 'ヴ' && curr != 'ャ' && curr != 'ィ' && curr != 'ュ' && curr != 'ェ' && curr != 'ョ'
                     && curr != 'ッ' && curr != 'ア' && curr != 'イ' && curr != 'ウ' && curr != 'エ' && curr != 'オ')
                {
                    finalName = finalName[0] + finalName;
                }
                finalStr += finalName;
                finalName = "";
            }
            finalStr += finalName;
            return finalStr;
        }

        private static string TranscriptInvertInternal(char curr, char next, char nnext, ref int i, ResourceManager transcriptionArray)
        {
            if (next != ' ')
            {
                if (nnext != ' ' && transcriptionArray.GetString("" + curr + next + nnext) != null)
                {
                    i++;
                    return transcriptionArray.GetString("" + curr + next + nnext);
                }
                if (curr == 'd' && next == 'o')
                    return transcriptionArray.GetString("_do");
                if (transcriptionArray.GetString("" + curr + next) != null)
                    return transcriptionArray.GetString("" + curr + next);
            }
            i--;
            if (transcriptionArray.GetString("" + curr) != null)
                return transcriptionArray.GetString("" + curr);
            return "" + curr;
        }

        private static bool IsRomanLetter(char c)
            => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');

        private static string ToHiraganaInternal(string name)
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
                    && curr == next && IsRomanLetter(curr))
                {
                    finalName += "っ";
                    i--;
                }
                else
                    finalName += TranscriptInvertInternal(curr, next, nnext, ref i, manager);
            }
            return finalName;
        }

        private static string ToKatakanaInternal(string name)
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
                    && curr == next && IsRomanLetter(curr))
                {
                    finalName += "ッ";
                    i--;
                }
                else if ((next == 'a' || next == 'i' || next == 'u' || next == 'e' || next == 'o')
                   && next == nnext)
                {
                    finalName += TranscriptInvertInternal(curr, next, nnext, ref i, manager) + "ー";
                    i++;
                }
                else
                    finalName += TranscriptInvertInternal(curr, next, nnext, ref i, manager);
            }
            return finalName;
        }
    }
}
