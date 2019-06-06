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

using BooruSharp.Booru;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class AnimePreload : APreload
    {
        public AnimePreload() : base(new[] { "anime" }, 30, Sentences.AnimeGame)
        { }

        public override bool IsNsfw()
            => false;

        public override bool DoesAllowFull()
            => true;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.SoloOnly;

        public override string GetRules(ulong guildId)
            => Sentences.RulesAnime(guildId);
    }

    public class Anime : AQuizz
    {
        public Anime(ITextChannel chan, Config config, ulong playerId) : base(chan, config.isFull ? Constants.animeDictionnaries.Item2 : Constants.animeDictionnaries.Item1, config, playerId)
        { }

        protected override void Init()
        {
            base.Init();
            _booru = new Sakugabooru();
        }

        protected override bool IsDictionnaryFull()
            => false;

        protected override bool DoesDisplayHelp()
            => true;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            var result = Features.NSFW.Booru.SearchBooru(false, new string[] { curr, "animated" }, _booru, Program.p.rand).GetAwaiter().GetResult();
            return (new Tuple<string[], string[]>(
                new[] { result.answer.url },
                result.answer.tags.Where(x => _booru.GetTag(x).GetAwaiter().GetResult().type == BooruSharp.Search.Tag.TagType.Copyright).ToArray()
            ));
        }

        private Sakugabooru _booru;

        public static Tuple<List<string>, List<string>> LoadDictionnaries()
        {
            if (!File.Exists("Saves/AnimeTags.dat"))
                return (null);
            List<string> tags = new List<string>();
            List<string> tagsFull = new List<string>();
            string[] allLines = File.ReadAllLines("Saves/AnimeTags.dat");
            foreach (string line in allLines)
            {
                string[] parts = line.Split(' ');
                if (int.Parse(parts[1]) > 10)
                    tags.Add(line.Split(' ')[0]);
                tagsFull.Add(line.Split(' ')[0]);
            }
            return (new Tuple<List<string>, List<string>>(tags, tagsFull));
        }
    }
}
