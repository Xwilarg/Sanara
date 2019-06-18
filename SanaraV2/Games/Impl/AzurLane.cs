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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class AzurLanePreload : APreload
    {
        public AzurLanePreload() : base(new[] { "azurlane", "al" }, 15, Sentences.AzurLaneGame)
        { }

        public override bool IsNsfw()
            => false;

        public override bool DoesAllowFull()
            => false;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.SoloOnly;

        public override string GetRules(ulong guildId, bool _)
            => Sentences.RulesKancolle(guildId);
    }

    public class AzurLane : AQuizz
    {
        public AzurLane(ITextChannel chan, Config config, ulong playerId) : base(chan, Constants.azurLaneDictionnary, config, playerId)
        { }

        protected override bool IsDictionnaryFull()
            => true;

        protected override bool DoesDisplayHelp()
            => false;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            JArray json;
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                json = JArray.Parse(hc.GetStringAsync("https://azurlane.koumakan.jp/w/api.php?action=opensearch&search=" + Uri.EscapeDataString(curr).Replace("%20", "+") + "&limit=1").GetAwaiter().GetResult());
            }
            string[] nameArray = json[3].ToObject<string[]>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                return (new Tuple<string[], string[]>(
                    new[] { "https://azurlane.koumakan.jp" + Regex.Match(hc.GetStringAsync(nameArray[0]).GetAwaiter().GetResult(),
                    "src=\"(\\/w\\/images\\/thumb\\/[^\\/]+\\/[^\\/]+\\/[^\\/]+\\/[0-9]+px-" + Uri.EscapeDataString(curr.Replace(" ", "_")) + ".png)").Groups[1].Value },
                    new[] { curr }
                ));
            }
        }

        public static ImmutableList<string> LoadDictionnary()
        {
            List<string> ships = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string html = hc.GetStringAsync("https://azurlane.koumakan.jp/List_of_Ships").GetAwaiter().GetResult();
                foreach (string s in html.Split(new string[] { "title=\"Category:" }, StringSplitOptions.None))
                {
                    if (s.Contains("Unreleased")) // We skip ships that weren't released and were found by data mining
                        continue;
                    Match match = Regex.Match(s, "<a href=\"\\/[^\"]+\" title=\"([^\"]+)\">[0-9]+<\\/a>");
                    if (match.Success)
                    {
                        string str = match.Groups[1].Value;
                        if (!ships.Contains(str))
                            ships.Add(str);
                    }
                }
            }
            return (ships.ToImmutableList());
        }
    }
}
