using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV3.Modules.Tool
{
    public sealed class LanguageModule : ModuleBase, IModule
    {
        public string GetModuleName()
            => "Tool";

        [Command("Japanese", RunMode = RunMode.Async)]
        public async Task Japanese([Remainder]string str)
        {
            JObject json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("http://jisho.org/api/v1/search/words?keyword=" + HttpUtility.UrlEncode(string.Join("%20", str))));
            var data = ((JArray)json["data"]).Select(x => x).ToArray();
            if (data.Length == 0)
                throw new CommandFailed("There is no result with this term search.");
            if (data.Length > 4)
                data = data.Take(5).ToArray();
            var embed = new EmbedBuilder
            {
                Color = Color.Blue,
                Title = str
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
            => ConvertLanguage(ConvertLanguage(entry, StaticObjects.HiraganaToRomaji, 'っ'), StaticObjects.KatakanaToRomaji, 'ッ');

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
                if (entry[0] == 'ー') // We can't really convert this katakana so we just ignore it
                {
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
