using Discord;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class BooruQuizzPreload : IPreload
    {
        public BooruQuizzPreload()
        {
            if (!File.Exists("Saves/Game/QuizzTags.txt"))
                File.WriteAllBytes("Saves/Game/QuizzTags.txt", StaticObjects.HttpClient.GetByteArrayAsync("https://files.zirk.eu/Sanara/QuizzTags.txt").GetAwaiter().GetResult());
            string[] lines = File.ReadAllLines("Saves/Game/QuizzTags.txt");
            var preload = new List<BooruQuizzPreloadResult>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] curr = lines[i].Split(' ');
                if (int.Parse(curr[1]) > 3)
                    preload.Add(new BooruQuizzPreloadResult(StaticObjects.Gelbooru, new[] { ".gif", ".png", ".jpg", ".jpeg" }, curr[0], new[] { curr[0] }));
            }
            _preload = preload.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "booruquizz", "booru" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzBooruTags(chan, user, this, settings);

        public string GetRules()
            => "I'll post 3 images, you'll have to give the tag they have in common.";

        public bool IsSafe()
            => false;

        private readonly BooruQuizzPreloadResult[] _preload;
    }
}
