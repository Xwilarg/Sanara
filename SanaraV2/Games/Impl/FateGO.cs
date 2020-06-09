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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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

        public override bool DoesAllowSendImage()
            => false;

        public override bool DoesAllowCropped()
            => true;

        public override Shadow DoesAllowShadow()
            => Shadow.None;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.Both;

        public override MultiplayerType GetMultiplayerType()
            => MultiplayerType.BestOf;

        public override string GetRules(IGuild guild, bool _)
            => Sentences.RulesCharacter(guild);
    }

    public class FateGO : AQuizz
    {
        public FateGO(IGuild guild, IMessageChannel chan, Config config, ulong playerId) : base(guild, chan, Constants.fateGODictionnary, config, playerId)
        { }

        protected override bool IsDictionnaryFull()
            => true;

        protected override bool DoesDisplayHelp()
            => false;

        private string CleanFateGOName(string name)
            => HttpUtility.UrlDecode(name).Replace("ō", "ou").Replace("ū", "uu").Replace("á", "a").Replace("ú", "u").Replace("ó", "o").Replace("é", "e").Replace("è", "e").Replace("ð", "d").Replace("&Amp;", "And").Replace("&#39;", "'");

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            using (HttpClient hc = new HttpClient())
            {
                string html = await hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/" + curr);

                List<string> allAnswer = new List<string>();
                allAnswer.Add(HttpUtility.UrlDecode(curr).Replace("&amp;", "And").Replace("&#39;", "'"));
                allAnswer.Add(CleanFateGOName(curr));
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
                        foreach (string sName in name.Split(','))
                        {
                            if (!string.IsNullOrWhiteSpace(sName))
                            {
                                allAnswer.Add(sName.Trim());
                                allAnswer.Add(CleanFateGOName(sName.Trim()));
                            }
                        }
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
                foreach (string servantClass in new[] { "Shielder", "Saber", "Archer", "Lancer", "Rider", "Caster", "Assassin", "Berserker", "Ruler", "Avenger", "Moon_Cancer", "Alter_Ego", "Foreigner" })
                {
                    string html = hc.GetStringAsync("https://fategrandorder.fandom.com/wiki/" + servantClass).GetAwaiter().GetResult();
                    html = html.Split(new[] { "navbox mw-collapsible" }, StringSplitOptions.None)[0]; // Remove useless things at ending
                    html = string.Join("", html.Split(new[] { "article-thumb tnone show-info-icon" }, StringSplitOptions.None).Skip(1));
                    foreach (string s in html.Split(new[] { "<td>" }, StringSplitOptions.None))
                    {
                        Match match = Regex.Match(s, "<a href=\"\\/wiki\\/([^\"]+)\"( |\t)*title=\"[^\"]+\">");
                        if (match.Success && !s.Contains("Unplayable"))
                        {
                            string name = match.Groups[1].Value;
                            if (!characters.Contains(name))
                            {
                                characters.Add(name);
                            }
                        }
                    }

                }
            }
            return (characters);
        }
    }
}
