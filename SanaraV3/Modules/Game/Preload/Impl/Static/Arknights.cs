using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SanaraV3.Modules.Game.Preload.Impl.Static
{
    public static class Arknights
    {
        public static List<(string, List<string>)> GetOperators()
            => _operators;

        static Arknights()
        {
            _operators = new List<(string, List<string>)>();
            var json = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(StaticObjects.HttpClient.GetStringAsync("https://aceship.github.io/AN-EN-Tags/json/gamedata/zh_CN/gamedata/excel/character_table.json").GetAwaiter().GetResult());
            var jsonName = JsonConvert.DeserializeObject<JArray>(StaticObjects.HttpClient.GetStringAsync("https://aceship.github.io/AN-EN-Tags/json/tl-unreadablename.json").GetAwaiter().GetResult());
            foreach (var elem in json)
            {
                string name = elem.Key;
                if (name.StartsWith("char_")) // char_ are characters (also contains the ones from the CN)
                {
                    List<string> names = new List<string>();
                    string appellation = elem.Value["appellation"];

                    if (appellation.StartsWith("Reserve")) // Event character from the CN, you can't play have them in your list
                        continue;

                    foreach (var n in jsonName)
                    {
                        if (n["name"].Value<string>() == appellation)
                        {
                            if (n["name"].Value<string>() == "Гум")
                                names.Add("Gummy"); // Somehow Gummy is wrote as "Gum"
                            names.Add(n["name_en"].Value<string>());
                            break;
                        }
                    }
                    names.Add(appellation);
                    _operators.Add((elem.Key, names));
                }
            }
        }

        private static List<(string, List<string>)> _operators;
    }
}
