using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Modules.Game.Preload.Result;
using System.Collections.Generic;
using System.Linq;

namespace SanaraV3.Modules.Game.Preload.Impl
{
    public sealed class ArknightsPreload : IPreload
    {
        public ArknightsPreload()
        {
            List<(string, List<string>)> operators = new List<(string, List<string>)>();
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
                                names.Add("GUMMY"); // Somehow Gummy is wrote as "Gum"
                            names.Add(n["name_en"].Value<string>().ToUpper());
                            break;
                        }
                    }
                    names.Add(appellation.ToUpper());
                    operators.Add((elem.Key, names));
                }
            }
            _preload = operators.Select((x) =>
            {
                return new QuizzPreloadResult("https://aceship.github.io/AN-EN-Tags/img/characters/" + x.Item1 + "_1.png", x.Item2.ToArray());
            }).ToArray();
        }

        public IPreloadResult[] Load()
            => _preload.Cast<IPreloadResult>().ToArray();

        public string[] GetGameNames()
            => new[] { "arknights", "ak" };

        public AGame CreateGame(IMessageChannel chan, GameSettings settings)
            => new Quizz(chan, this, settings);

        private readonly QuizzPreloadResult[] _preload;
    }
}
