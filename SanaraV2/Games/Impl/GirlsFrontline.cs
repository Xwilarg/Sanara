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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class GirlsFrontlinePreload : APreload
    {
        public GirlsFrontlinePreload() : base(new[] { "girlsfrontline", "gf" }, 15, Sentences.GirlsFrontlineGame)
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
            => Shadow.Transparency;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.Both;

        public override MultiplayerType GetMultiplayerType()
            => MultiplayerType.BestOf;

        public override string GetRules(IGuild guild, bool _)
            => Sentences.RulesGirlsFrontline(guild);
    }

    public class GirlsFrontline : AQuizz
    {
        public GirlsFrontline(IGuild guild, IMessageChannel chan, Config config, ulong playerId) : base(guild, chan, Constants.girlsfrontlineDictionnary, config, playerId)
        { }

        protected override bool IsDictionnaryFull()
            => true;

        protected override bool DoesDisplayHelp()
            => false;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            using (HttpClient hc = new HttpClient())
            {
                string html = await hc.GetStringAsync("https://en.gfwiki.com/wiki/File:" + curr + ".png"); // Dictionnary already contains name corresponding to the URL
                Match m = Regex.Match(html, "src=\"(\\/images\\/thumb\\/[^\"]+)\"");
                return (new Tuple<string[], string[]>(
                    new[] { "https://en.gfwiki.com" + m.Groups[1].Value },
                    new[] { curr.Replace("%E2%88%95", "/") }
                ));
            }
        }

        public static List<string> LoadDictionnary()
        {
            List<string> tDolls = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                string html = hc.GetStringAsync("https://en.gfwiki.com/wiki/T-Doll_Index").GetAwaiter().GetResult();
                html = html.Split(new[] { "Unreleased_T-Dolls_(T-Dolls_without_index_number)" }, StringSplitOptions.None)[0]; // We remove T-Doll that weren't release in the game
                MatchCollection match = Regex.Matches(html, "<a href=\"\\/wiki\\/([^\"]+)\" title=\"[^\"]+\"><img"); // Getting all T-Dolls
                return match.Cast<Match>().Select(x => x.Groups[1].Value).ToList();
            }
        }
    }
}
