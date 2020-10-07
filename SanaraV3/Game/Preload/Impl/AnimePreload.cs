using Discord;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Result;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class AnimePreload : IPreload
    {
        public AnimePreload()
        {
            if (!File.Exists("Saves/Game/QuizzAnime.txt"))
                File.WriteAllBytes("Saves/Game/QuizzAnime.txt", StaticObjects.HttpClient.GetByteArrayAsync("https://files.zirk.eu/Sanara/QuizzAnime.txt").GetAwaiter().GetResult());
            string[] lines = File.ReadAllLines("Saves/Game/QuizzAnime.txt");
            var preload = new List<BooruQuizzPreloadResult>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] curr = lines[i].Split(' ');
                if (int.Parse(curr[1]) > 10)
                    preload.Add(new BooruQuizzPreloadResult(StaticObjects.Sakugabooru, new[] { ".mp4", ".webm" }, curr[0], new[] { curr[0] }));
            }
            _preload = preload.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "anime" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzBooru(chan, user, this, settings);

        private readonly BooruQuizzPreloadResult[] _preload;
    }
}
