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
using Discord.Audio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class ArknightsAudioPreload : APreload
    {
        public ArknightsAudioPreload() : base(new[] { "arkaudio" }, 15, Sentences.ArknightsAudioGame)
        { }

        public override bool IsNsfw()
            => false;

        public override bool DoesAllowFull()
            => false;

        public override bool DoesAllowSendImage()
            => false;

        public override bool DoesAllowCropped()
            => false;

        public override Shadow DoesAllowShadow()
            => Shadow.None;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.Both;

        public override MultiplayerType GetMultiplayerType()
            => MultiplayerType.BestOf;

        public override string GetRules(IGuild guild, bool _)
            => Sentences.RulesArknightsAudio(guild);
    }

    public class ArknightsAudio : AQuizz
    {
        public ArknightsAudio(IGuild guild, IMessageChannel chan, Config config, ulong playerId) : base(guild, chan, Constants.arknightsDictionnary, config, playerId)
        { }

        protected override bool IsDictionnaryFull()
            => true;

        protected override bool DoesDisplayHelp()
            => false;

        protected override PostType GetPostType()
            => PostType.Audio;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            dynamic json = JsonConvert.DeserializeObject(await _http.GetStringAsync("https://aceship.github.io/AN-EN-Tags/json/gamedata/zh_CN/gamedata/excel/character_table.json"));
            dynamic jsonName = JsonConvert.DeserializeObject(await _http.GetStringAsync("https://aceship.github.io/AN-EN-Tags/json/tl-unreadablename.json"));
            List<string> names = new List<string>();
            string appelation = json[curr].appellation;
            foreach (dynamic name in jsonName)
            {
                if (name.name == appelation)
                    names.Add((string)name.name_en);
            }
            names.Add(appelation);
            names.Add((string)json[curr].name);
            return (new Tuple<string[], string[]>(
                new[] { "https://aceship.github.io/AN-EN-Tags/etc/voice/" + curr + "/CN_042.mp3" },
                names.ToArray()
            ));
        }

        public static List<string> LoadDictionnary()
        {
            List<string> operators = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(hc.GetStringAsync("https://aceship.github.io/AN-EN-Tags/json/gamedata/zh_CN/gamedata/excel/character_table.json").GetAwaiter().GetResult());
                foreach (var elem in json)
                {
                    string name = elem.Key;
                    if (name.StartsWith("char_"))
                    {
                        operators.Add(elem.Key);
                    }
                }
            }
            return operators;
        }
    }
}
