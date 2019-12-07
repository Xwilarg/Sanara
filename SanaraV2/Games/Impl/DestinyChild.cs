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
    public class DestinyChildPreload : APreload
    {
        public DestinyChildPreload() : base(new[] { "destinychild", "destiny" }, 15, Sentences.DestinyChildGame)
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

        public override string GetRules(ulong guildId, bool _)
            => Sentences.RulesDestinyChild(guildId);
    }

    public class DestinyChild : AQuizz
    {
        public DestinyChild(ITextChannel chan, Config config, ulong playerId) : base(chan, Constants.destinyChildDictionnary, config, playerId)
        { }

        protected override bool IsDictionnaryFull()
            => true;

        protected override bool DoesDisplayHelp()
            => false;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            string html;
            using (HttpClient hc = new HttpClient())
            {
                html = await hc.GetStringAsync("https://destiny-child-for-kakao.fandom.com/wiki/" + curr);
            }
            using (HttpClient hc = new HttpClient())
            {
                return (new Tuple<string[], string[]>(
                    new[] { Regex.Match(html, "<meta property=\"og:image\" content=\"([^\"]+)\"").Groups[1].Value.Split(new[] { ".png" }, StringSplitOptions.None)[0] + ".png" },
                    new[]
                    {
                        Regex.Match(html, "<meta property=\"og:title\" content=\"([^\"]+)\"").Groups[1].Value
                    }
                ));
            }
        }

        public static List<string> LoadDictionnary()
        {
            List<string> children = new List<string>();
            string[] urls = new string[]
            {
                "https://destiny-child-for-kakao.fandom.com/wiki/Category:Attacker_Type",
                "https://destiny-child-for-kakao.fandom.com/wiki/Category:Debuffer_Type",
                "https://destiny-child-for-kakao.fandom.com/wiki/Category:Defender_Type",
                "https://destiny-child-for-kakao.fandom.com/wiki/Category:Healer_Type",
                "https://destiny-child-for-kakao.fandom.com/wiki/Category:Supporter_Type"
            };
            string htmlRef; // Contains a way to know if characters are childs or monsters
            using (HttpClient hc = new HttpClient())
            {
                htmlRef = hc.GetStringAsync("https://lokicoder.github.io/destiny-child-tools/bundle.js").GetAwaiter().GetResult()
                            .Split(new[] { "@license MIT" }, StringSplitOptions.None)[1].Split(new[] { "@license React v16.11.0" }, StringSplitOptions.None)[0];
                foreach (string url in urls)
                {
                    string html = hc.GetStringAsync(url).GetAwaiter().GetResult();
                    foreach (Match m in Regex.Matches(html, "<a href=\"\\/wiki\\/([^\"]+)\" title=\"([^\"]+)\">\\n\\t+<img").Cast<Match>())
                    {
                        if (Regex.Match(htmlRef, "c\\d+:{([a-zA-Z]+:[^,]+,)*name:\"" + m.Groups[2].Value.Replace("&#039;", "'"), RegexOptions.IgnoreCase).Success)
                            children.Add(m.Groups[1].Value);
                    }
                }
            }
            return children;
        }
    }
}
