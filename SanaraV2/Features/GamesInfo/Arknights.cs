using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Features.GamesInfo
{
    public class Arknights
    {
        public static async Task<FeatureRequest<Response.ArknightsCharac, Error.Charac>> SearchCharac(string[] args)
        {
            if (args.Length == 0)
                return new FeatureRequest<Response.ArknightsCharac, Error.Charac>(null, Error.Charac.Help);
            string name = Utilities.CleanWord(string.Join(" ", args).ToLower());
            using (HttpClient hc = new HttpClient())
            {
                dynamic json = JsonConvert.DeserializeObject(await hc.GetStringAsync("https://aceship.github.io/AN-EN-Tags/json/tl-unreadablename.json"));
                foreach (dynamic elem in json)
                {
                    if (name == Utilities.CleanWord((string)elem.name_en))
                    {
                        name = Utilities.CleanWord((string)elem.name);
                        break;
                    }
                }
                var fullJson = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(await hc.GetStringAsync("https://aceship.github.io/AN-EN-Tags/json/gamedata/zh_CN/gamedata/excel/character_table.json"));
                foreach (var elem in fullJson)
                {
                    if (elem.Key.StartsWith("char_") && name == Utilities.CleanWord((string)elem.Value.appellation))
                    {
                        return new FeatureRequest<Response.ArknightsCharac, Error.Charac>(new Response.ArknightsCharac()
                        {
                            name = elem.Value.appellation,
                            imgUrl = "https://aceship.github.io/AN-EN-Tags/img/characters/" + elem.Key + "_1.png"
                        }, Error.Charac.None);
                    }
                }
                return new FeatureRequest<Response.ArknightsCharac, Error.Charac>(null, Error.Charac.NotFound);
            }
        }
    }
}
