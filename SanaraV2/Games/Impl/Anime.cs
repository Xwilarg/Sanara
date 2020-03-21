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
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        public override bool DoesAllowSendImage()
            => true;

        public override bool DoesAllowCropped()
            => false;

        public override Shadow DoesAllowShadow()
            => Shadow.None;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.Both;

        public override MultiplayerType GetMultiplayerType()
            => MultiplayerType.BestOf;

        public override string GetRules(ulong guildId, bool _)
            => Sentences.RulesAnime(guildId);
    }

    public class Anime : AQuizz
    {
        public Anime(ITextChannel chan, Config config, ulong playerId) : base(chan, config.isFull ? Constants.animeDictionnaries.Item2 : Constants.animeDictionnaries.Item1, config, playerId)
        {
            _sendImage = config.sendImage;
            if (_sendImage && Directory.Exists("Saves"))
                Directory.CreateDirectory("Saves");
        }

        protected override void Init()
        {
            base.Init();
            _booru = new Sakugabooru();
        }

        protected override bool IsDictionnaryFull()
            => false;

        protected override bool DoesDisplayHelp()
            => true;

        protected override PostType GetPostType()
            => _sendImage ? PostType.LocalPath : PostType.Url;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            var result = Features.NSFW.Booru.SearchBooru(false, new string[] { curr, "animated" }, _booru, Program.p.rand).GetAwaiter().GetResult();
            var answers = result.answer.tags.Where(x => _booru.GetTagAsync(x).GetAwaiter().GetResult().type == BooruSharp.Search.Tag.TagType.Copyright).ToArray();
            if (_sendImage)
            {
                if (result.answer.url.EndsWith(".gif"))
                    return await GetPostInternalAsync(curr); // We only take .mp4 for now
                string randomPath = GetRandomPath();
                string path = "Saves/" + randomPath + ".mp4";
                using (HttpClient hc = new HttpClient())
                    File.WriteAllBytes(path, await hc.GetByteArrayAsync(result.answer.url));
                using (var engine = new Engine())
                {
                    var mp4 = new MediaFile { Filename = path };
                    engine.GetMetadata(mp4);
                    int middle = (int)mp4.Metadata.Duration.TotalSeconds / 2;
                    int lowerQuartile = middle / 2;
                    int upperQuartile = lowerQuartile * 3;
                    engine.GetThumbnail(mp4, new MediaFile { Filename = "Saves/" + randomPath + "-1.jpeg" },
                        new ConversionOptions { Seek = TimeSpan.FromSeconds(lowerQuartile) });
                    engine.GetThumbnail(mp4, new MediaFile { Filename = "Saves/" + randomPath + "-2.jpeg" },
                        new ConversionOptions { Seek = TimeSpan.FromSeconds(middle) });
                    engine.GetThumbnail(mp4, new MediaFile { Filename = "Saves/" + randomPath + "-3.jpeg" },
                        new ConversionOptions { Seek = TimeSpan.FromSeconds(upperQuartile) });
                }
                File.Delete(path);
                return (new Tuple<string[], string[]>(
                    new[] { "Saves/" + randomPath + "-1.jpeg", "Saves/" + randomPath + "-2.jpeg", "Saves/" + randomPath + "-3.jpeg" }, answers
                ));
            }
            else
            {
                return (new Tuple<string[], string[]>(
                    new[] { result.answer.url }, answers
                ));
            }
        }

        private Sakugabooru _booru;

        public static Tuple<List<string>, List<string>> LoadDictionnaries()
        {
            if (!File.Exists("Saves/AnimeTags.dat"))
                return new Tuple<List<string>, List<string>>(new List<string>(), new List<string>());
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

        private bool _sendImage; // Send 3 images instead of the video
    }
}
