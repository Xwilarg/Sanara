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
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class BooruPreload : APreload
    {
        public BooruPreload() : base(new[] { "booru" }, 45, Sentences.BooruGame)
        { }

        public override bool IsNsfw()
            => true;

        public override bool DoesAllowFull()
            => false;

        public override bool DoesAllowSendImage()
            => false;

        public override bool DoesAllowCropped() // Make no sense to crop booru images when the tag can be anywhere
            => false;

        public override Shadow DoesAllowShadow()
            => Shadow.None;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.SoloOnly;

        public override string GetRules(ulong guildId, bool _)
            => Sentences.RulesBooru(guildId);
    }

    public class Booru : AQuizz
    {
        public Booru(ITextChannel chan, Config config, ulong playerId) : base(chan, Constants.booruDictionnary, config, playerId)
        { }

        protected override void Init()
        {
            base.Init();
            _booru = new Gelbooru();
        }

        protected override bool IsDictionnaryFull()
            => false;

        protected override bool DoesDisplayHelp()
            => true;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            return (new Tuple<string[], string[]>(
                new[] { Features.NSFW.Booru.SearchBooru(false, new string[] { curr }, _booru, Program.p.rand).GetAwaiter().GetResult().answer.url,
                 Features.NSFW.Booru.SearchBooru(false, new string[] { curr }, _booru, Program.p.rand).GetAwaiter().GetResult().answer.url,
                 Features.NSFW.Booru.SearchBooru(false, new string[] { curr }, _booru, Program.p.rand).GetAwaiter().GetResult().answer.url},
                new[] { curr }
            ));
        }

        private Gelbooru _booru;

        public static List<string> LoadDictionnary()
        {
            if (!File.Exists("Saves/BooruTriviaTags.dat"))
                return (new List<string>());
            List<string> tags = new List<string>();
            string[] allLines = File.ReadAllLines("Saves/BooruTriviaTags.dat");
            foreach (string line in allLines)
            {
                string[] linePart = line.Split(' ');
                if (Convert.ToInt32(linePart[1]) >= 3)
                    tags.Add(linePart[0]);
            }
            return (tags);
        }
    }
}
