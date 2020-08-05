using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
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
                    string word = x["word"].Value<string>();
                    if (word == null)
                        return x["reading"].Value<string>();
                    return word + " - " + x["reading"].Value<string>();
                }));
                embed.AddField(title, content);
            }
            await ReplyAsync(embed: embed.Build());
        }

        /// <summary>
        /// Convert an entry from a language to another
        /// </summary>
        /// <param name="entry">The entry to translate</param>
        /// <param name="manager">The resource manager that contains the from/to for each character</param>
        public static string ConvertLanguage(string entry, ResourceManager manager)
        {
            StringBuilder result = new StringBuilder();
            while (entry.Length > 0)
            {
                // Iterate on 3 to 1 (We assume that 3 is the max number of character)
                // We then test for each entry if we can convert
                // We begin with the biggest, if we don't do so, we would find ん (n) before な (na)
                for (int i = 3; i > 0; i++)
                {
                    if (entry.Length >= i)
                    {
                        string value = manager.GetString(entry[0..i]);
                        if (value != null)
                            result.Append(value);
                        entry = entry.Substring(i);
                        goto found;
                    }
                }
                result.Append(entry[0]);
                entry = entry.Substring(1);
            found:;
            }
            return result.ToString();
        }
    }
}
