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

using Discord;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class FateGOPreload : APreload
    {
        public FateGOPreload() : base(new[] { "fatego", "fgo", "fate", "fategrandorder" }, 15, Sentences.FateGOGame)
        { }

        public override bool IsNsfw()
            => false;

        public override bool DoesAllowFull()
            => false;

        public override string GetRules(ulong guildId)
            => Sentences.RulesCharacter(guildId);
    }

    public class FateGO : AQuizz
    {
        public FateGO(ITextChannel chan, Config config) : base(chan, Constants.fateGODictionnary, config)
        { }

        protected override bool IsDictionnaryFull()
            => true;

        protected override bool DoesDisplayHelp()
            => false;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            using (HttpClient hc = new HttpClient())
            {
                string html = await hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Special:Search?search=" + Uri.EscapeDataString(curr) + "&limit=1");
                html = await hc.GetStringAsync(Regex.Match(html, "<a href=\"(https:\\/\\/fategrandorder\\.fandom\\.com\\/wiki\\/[^\"]+)\" class=\"result-link").Groups[1].Value);

                List<string> allAnswer = new List<string>();
                allAnswer.Add(curr.Replace("&Amp;", "And").Replace("&#39;", "'"));
                allAnswer.Add(curr.Replace("ō", "ou").Replace("á", "a").Replace("ú", "u").Replace("ó", "o").Replace("é", "e").Replace("ð", "d").Replace("&Amp;", "And").Replace("&#39;", "'"));
                if (html.Contains("AKA:"))
                {
                    foreach (string s in Regex.Replace(html.Split(new[] { "AKA:" }, StringSplitOptions.None)[1].Split(new[] { "</table>" }, StringSplitOptions.None)[0], "\\([^\\)]+\\)", "").Split(','))
                    {
                        string name = s;
                        Match m = Regex.Match(name, "<[^>]+>([^<]+)<\\/[^>]+>");
                        if (m.Success)
                            name = m.Groups[1].Value;
                        name = Regex.Replace(name, "<[^>]+>", "");
                        name = Regex.Replace(name, "<\\/[^>]+>", "");
                        if (!string.IsNullOrWhiteSpace(name))
                            allAnswer.Add(name.Trim());
                    }
                }
                return (new Tuple<string[], string[]>(
                    new[] { Regex.Match(html.Split(new[] { "pi-image-collection-tab-content current" }, StringSplitOptions.None)[1], "<a href=\"([^\"]+)\"").Groups[1].Value.Split(new string[] { "/revision" }, StringSplitOptions.None)[0] },
                    allAnswer.ToArray()
                ));
            }
        }

        public static List<string> LoadDictionnary()
        {
            List<string> characters = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string html = hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/Servant_List_by_ID").GetAwaiter().GetResult();
                foreach (string s in html.Split(new[] { "<tr>" }, StringSplitOptions.None))
                {
                    Match match = Regex.Match(s, "<a href=\"\\/wiki\\/[^\"]+\"[ \\t]+class=\"[^\"]+\"[ \\t]+title=\"([^\"]+)\"");
                    if (match.Success)
                    {
                        string name = match.Groups[1].Value;
                        if (!s.Contains("Unplayable Servants") && !characters.Contains(name))
                            characters.Add(name);
                    }
                }
            }
            return (characters);
        }
    }
}
