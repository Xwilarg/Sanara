using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
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
                if (Program.p.ARKNIGHTS_ALIASES.ContainsKey(name))
                {
                    name = Program.p.ARKNIGHTS_ALIASES[name];
                }
                var fullJson = Program.p.ARKNIGHTS_GENERAL;
                foreach (var elem in fullJson)
                {
                    if (elem.Key.StartsWith("char_") && name == Utilities.CleanWord((string)elem.Value.appellation))
                    {
                        var skills = new List<Response.ArknightsSkill>();
                        foreach (dynamic skill in elem.Value.skills)
                        {
                            var skillArr = Program.p.ARKNIGHTS_SKILLS[(string)skill.skillId];
                            skills.Add(new Response.ArknightsSkill { name = skillArr.Item1, description = skillArr.Item2 });
                        }
                        return new FeatureRequest<Response.ArknightsCharac, Error.Charac>(new Response.ArknightsCharac()
                        {
                            name = elem.Value.appellation,
                            imgUrl = "https://aceship.github.io/AN-EN-Tags/img/characters/" + elem.Key + "_1.png",
                            type = ToSentenceCase((string)elem.Value.position),
                            tags = ((JArray)elem.Value.tagList).Select(x => ToSentenceCase(Program.p.ARKNIGHTS_TAGS[x.Value<string>()])).ToArray(),
                            wikiUrl = "https://aceship.github.io/AN-EN-Tags/akhrchars.html?opname=" + ((string)elem.Value.appellation).Replace(' ', '_'),
                            skills = skills.ToArray(),
                            description = Program.p.ARKNIGHTS_DESCRIPTIONS[(string)elem.Value.appellation]
                        }, Error.Charac.None);
                    }
                }
                return new FeatureRequest<Response.ArknightsCharac, Error.Charac>(null, Error.Charac.NotFound);
            }
        }

        private static string ToSentenceCase(string str)
            => str[0] + string.Join("", str.Skip(1)).ToLower();
    }
}
