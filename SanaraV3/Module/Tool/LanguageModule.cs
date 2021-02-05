using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordUtils;
using Google;
using Google.Cloud.Vision.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exception;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadLanguageHelp()
        {
            _submoduleHelp.Add("Language", "Get various information related to others languages");
            _help.Add(("Tool", new Help("Language", "Japanese", new[] { new Argument(ArgumentType.MANDATORY, "word") }, "Get the meaning of a Japanese word, will also translate your word if you give it in english.", new string[0], Restriction.None, "Japanese submarine")));
            _help.Add(("Tool", new Help("Language", "Kanji", new[] { new Argument(ArgumentType.MANDATORY, "kanji") }, "Get information about a kanji.", new string[0], Restriction.None, "Kanji 艦")));
            _help.Add(("Tool", new Help("Language", "Urban", new[] { new Argument(ArgumentType.MANDATORY, "word") }, "Get the urban definition of a word.", new string[0], Restriction.Nsfw, "Urban bunny hop")));
            _help.Add(("Tool", new Help("Language", "Translate", new[] { new Argument(ArgumentType.MANDATORY, "language"), new Argument(ArgumentType.MANDATORY, "sentence/image") }, "Translate a sentence to the given language.", new string[0], Restriction.None, "Translate en 空は青いです")));
        }
    }
}

namespace SanaraV3.Module.Tool
{
    public sealed class LanguageModule : ModuleBase
    {
        public static async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel chan, SocketReaction react)
        {
            string emote = react.Emote.ToString();
            bool allowFlags = chan is ITextChannel textChan && StaticObjects.Db.GetGuild(textChan.GuildId).TranslateUsingFlags;
            // If emote is not from the bot and is an arrow emote
            if (allowFlags && react.User.IsSpecified && react.User.Value.Id != StaticObjects.ClientId && StaticObjects.Flags.ContainsKey(emote))
            {
                var gMsg = (await msg.GetOrDownloadAsync()).Content;
                if (!string.IsNullOrEmpty(gMsg))
                {
                    var translation = await StaticObjects.TranslationClient.TranslateTextAsync(gMsg, StaticObjects.Flags[emote]);
                    await chan.SendMessageAsync(embed: new EmbedBuilder
                    {
                        Title = "From " + (StaticObjects.ISO639.ContainsKey(translation.DetectedSourceLanguage) ? StaticObjects.ISO639[translation.DetectedSourceLanguage] : translation.DetectedSourceLanguage),
                        Description = translation.TranslatedText,
                        Color = Color.Blue
                    }.Build());
                }
            }
        }

        [Command("Translate", RunMode = RunMode.Async)]
        public async Task TranslateAsync(string language)
        {
            if (StaticObjects.ISO639Reverse.ContainsKey(language))
                language = StaticObjects.ISO639Reverse[language];

            if (language.Length != 2)
                throw new CommandFailed("The language given must be in format ISO 639-1.");


            if (Context.Message.Attachments.Count == 0)
                throw new CommandFailed("No text or image was attached.");

            if (!Utils.IsImage(Path.GetExtension(Context.Message.Attachments.First().Url)))
                throw new CommandFailed("The file attached was not an image");

            try
            {
                var image = await Google.Cloud.Vision.V1.Image.FetchFromUriAsync(Context.Message.Attachments.ElementAt(0).Url);
                TextAnnotation response;
                try
                {
                    response = await StaticObjects.VisionClient.DetectDocumentTextAsync(image);
                }
                catch (AnnotateImageException)
                {
                    throw new CommandFailed("The file given isn't a valid image.");
                }
                if (response == null)
                    throw new CommandFailed("There is no text on the image.");

                var translation = await StaticObjects.TranslationClient.TranslateTextAsync(response.Text, language);
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "From " + (StaticObjects.ISO639.ContainsKey(translation.DetectedSourceLanguage) ? StaticObjects.ISO639[translation.DetectedSourceLanguage] : translation.DetectedSourceLanguage),
                    Description = translation.TranslatedText,
                    Color = Color.Blue
                }.Build());
            }
            catch (GoogleApiException)
            {
                throw new CommandFailed("The language you provided is invalid.");
            }
        }

        [Command("Translate", RunMode = RunMode.Async)]
        public async Task TranslateAsync(string language, [Remainder]string sentence)
        {
            if (StaticObjects.ISO639Reverse.ContainsKey(language))
                language = StaticObjects.ISO639Reverse[language];

            if (language.Length != 2)
                throw new CommandFailed("The language given must be in format ISO 639-1.");

            try
            {
                var translation = await StaticObjects.TranslationClient.TranslateTextAsync(sentence, language);
                await ReplyAsync(embed: new EmbedBuilder
                {
                    Title = "From " + (StaticObjects.ISO639.ContainsKey(translation.DetectedSourceLanguage) ? StaticObjects.ISO639[translation.DetectedSourceLanguage] : translation.DetectedSourceLanguage),
                    Description = translation.TranslatedText,
                    Color = Color.Blue
                }.Build());
            }
            catch (GoogleApiException)
            {
                throw new CommandFailed("The language you provided is invalid.");
            }
        }

        [Command("Urban", RunMode = RunMode.Async), RequireNsfw]
        public async Task UrbanAsync([Remainder]string query)
        {
            var json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + HttpUtility.UrlEncode(query)));
            if (json["list"].Value<JArray>().Count == 0)
                throw new CommandFailed("There is no definition for this query.");
            string definition = json["list"][0]["definition"].Value<string>();
            if (definition.Length > 1000)
                definition = definition.Substring(0, 1000) + " [...]";
            string example = json["list"][0]["example"].Value<string>();
            if (example.Length > 1000)
                example = example.Substring(0, 1000) + " [...]";
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                Title = char.ToUpper(query[0]) + string.Concat(query.ToLower().Skip(1)),
                Url = json["list"][0]["permalink"].Value<string>(),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Definition",
                        Value = definition
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Example",
                        Value = example
                    }
                }
            }.Build());
        }

        [Command("Kanji", RunMode = RunMode.Async)]
        public async Task KanjiAsync(string kanji)
        {
            JObject json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("https://jisho.org/api/v1/search/words?keyword=" + HttpUtility.UrlEncode(kanji)));
            if (json["data"].Value<JArray>().Count == 0 || json["data"][0]["japanese"][0]["word"] == null)
                throw new CommandFailed("I didn't find any kanji with this query.");

            char? finalKanji = null;

            // If player input is a kanji
            char playerChar = kanji[0];
            foreach (var elem in json["data"].Value<JArray>())
            {
                foreach (var elem2 in elem["japanese"].Value<JArray>())
                {
                    if (elem2["word"] != null && elem2["word"].Value<string>() == playerChar.ToString())
                    {
                        finalKanji = playerChar;
                    }
                }
            }

            // Else we take the first result we got on our Jisho search
            if (!finalKanji.HasValue)
                finalKanji = json["data"][0]["japanese"][0]["word"].Value<string>()[0];

            string url = "https://jisho.org/search/" + finalKanji + "%20%23kanji";
            string html = await StaticObjects.HttpClient.GetStringAsync(url);

            // Radical of the kanji
            var radicalMatch = Regex.Match(html, "<span class=\"radical_meaning\">([^<]+)<\\/span>([^<]+)<\\/span>");

            // Kanji meaning in english
            var meaning = Regex.Match(html, "<div class=\"kanji-details__main-meanings\">([^<]+)<\\/div>").Groups[1].Value.Trim();

            // All parts composing the kanji
            Dictionary<string, string> parts = new Dictionary<string, string>();
            foreach (var match in Regex.Matches(html, "<a href=\"(\\/\\/jisho\\.org\\/search\\/[^k]+kanji)\">([^<]+)<\\/a>").Cast<Match>())
            {
                string name = match.Groups[2].Value;
                if (name[0] == finalKanji.Value)
                    parts.Add(name, meaning);
                else
                    parts.Add(name, Regex.Match(await StaticObjects.HttpClient.GetStringAsync("https:" + match.Groups[1].Value), "<div class=\"kanji-details__main-meanings\">([^<]+)<\\/div>").Groups[1].Value.Trim());
            }

            // Onyomi and kunyomi (ways to read the kanji)
            Dictionary<string, string> onyomi = new Dictionary<string, string>();
            Dictionary<string, string> kunyomi = new Dictionary<string, string>();
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

            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = finalKanji.Value.ToString(),
                // Url = url, // TODO: https://github.com/dotnet/runtime/issues/21626 , Discord.NET said they will fix that in a next release
                Description = meaning,
                ImageUrl = "http://classic.jisho.org/static/images/stroke_diagrams/" + (int)finalKanji.Value + "_frames.png",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Radical",
                        Value = radicalMatch.Groups[2].Value.Trim() + ": " + radicalMatch.Groups[1].Value.Trim()
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Parts",
                        Value = string.Join("\n", parts.Select(x => x.Value == "" ? x.Key : x.Key + ": " + x.Value))
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Onyomi",
                        Value = onyomi.Count == 0 ? "None" : string.Join("\n", onyomi.Select(x => x.Key + " (" + x.Value + ")"))
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Kunyomi",
                        Value = kunyomi.Count == 0 ? "None" : string.Join("\n", kunyomi.Select(x => x.Key + " (" + x.Value + ")"))
                    },
                }
            }.Build());
        }

        [Command("Japanese", RunMode = RunMode.Async)]
        public async Task JapaneseAsync([Remainder]string str)
        {
            JObject json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("http://jisho.org/api/v1/search/words?keyword="
                + HttpUtility.UrlEncode(string.Join("%20", str))));
            var data = ((JArray)json["data"]).Select(x => x).ToArray();
            if (data.Length == 0)
                throw new CommandFailed("There is no result with this term search.");
            if (data.Length > 4)
                data = data.Take(5).ToArray();
            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Title = str,
                Url = "https://jisho.org/search/" + HttpUtility.UrlEncode(str)
            };
            foreach (var elem in data)
            {
                string title = string.Join(", ", elem["senses"][0]["english_definitions"].Value<JArray>().Select(x => x.Value<string>()));
                string content = string.Join('\n', elem["japanese"].Value<JArray>().Select(x =>
                {
                    var word = x["word"];
                    var reading = x["reading"];
                    if (word == null)
                        return reading.Value<string>() + $" ({ToRomaji(reading.Value<string>())})";
                    if (reading == null)
                        return word.Value<string>();
                    return word.Value<string>() + " - " + reading.Value<string>() + $" ({ToRomaji(reading.Value<string>())})";
                }));
                embed.AddField(title, content);
            }
            await ReplyAsync(embed: embed.Build());
        }

        public static string ToRomaji(string entry)
            => ConvertLanguage(ConvertLanguage(entry, StaticObjects.KatakanaToRomaji, 'ッ'), StaticObjects.HiraganaToRomaji, 'っ');

        public static string ToHiragana(string entry)
            => ConvertLanguage(ConvertLanguage(entry, StaticObjects.KatakanaToRomaji, 'ッ'), StaticObjects.RomajiToHiragana, 'っ');

        /// <summary>
        /// Convert an entry from a language to another
        /// </summary>
        /// <param name="entry">The entry to translate</param>
        /// <param name="dictionary">The dictionary that contains the from/to for each character</param>
        /// <param name="doubleChar">Character to use when a character is here twice, like remplace kko by っこ</param>
        public static string ConvertLanguage(string entry, Dictionary<string, string> dictionary, char doubleChar)
        {
            StringBuilder result = new StringBuilder();
            var biggest = dictionary.Keys.OrderByDescending(x => x.Length).First().Length;
            bool isEntryRomaji = IsLatinLetter(dictionary.Keys.First()[0]);
            bool doubleNext; // If we find a doubleChar, the next character need to be doubled (っこ -> kko)
            while (entry.Length > 0)
            {
                doubleNext = false;

                // SPECIAL CASES FOR KATAKANA
                if (entry[0] == 'ー') // We can't really convert this katakana so we just ignore it
                {
                    entry = entry.Substring(1);
                    continue;
                }
                if (entry[0] == 'ァ' || entry[0] == 'ィ'|| entry[0] == 'ゥ' || entry[0] == 'ェ' || entry[0] == 'ォ')
                {
                    result.Remove(result.Length - 1, 1);
                    char tmp;
                    switch (entry[0])
                    {
                        case 'ァ': tmp = 'a'; break;
                        case 'ィ': tmp = 'i'; break;
                        case 'ゥ': tmp = 'u'; break;
                        case 'ェ': tmp = 'e'; break;
                        case 'ォ': tmp = 'o'; break;
                        default: throw new ArgumentException("Invalid katakana " + entry[0]);
                    }
                    result.Append(tmp);
                    entry = entry.Substring(1);
                    continue;
                }

                if (entry.Length >= 2 && entry[0] == entry[1] && isEntryRomaji) // kko -> っこ
                {
                    result.Append(doubleChar);
                    entry = entry.Substring(1);
                    continue;
                }
                if (entry[0] == doubleChar)
                {
                    doubleNext = true;
                    entry = entry.Substring(1);
                    if (entry.Length == 0)
                        continue;
                }
                // Iterate on biggest to 1 (We assume that 3 is the max number of character)
                // We then test for each entry if we can convert
                // We begin with the biggest, if we don't do so, we would find ん (n) before な (na)
                for (int i = biggest; i > 0; i--)
                {
                    if (entry.Length >= i)
                    {
                        var value = entry[0..i];
                        if (dictionary.ContainsKey(value))
                        {
                            if (doubleNext)
                                result.Append(dictionary[value][0]);
                            result.Append(dictionary[value]);
                            entry = entry.Substring(i);
                            goto found;
                        }
                    }
                }
                result.Append(entry[0]);
                entry = entry.Substring(1);
            found:;
            }
            return result.ToString();
        }

        private static bool IsLatinLetter(char c)
            => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}
