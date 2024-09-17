using BooruSharp.Booru;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class BooruQuizzPreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            if (!File.Exists("Saves/Game/QuizzTags.txt"))
                File.WriteAllBytes("Saves/Game/QuizzTags.txt", provider.GetRequiredService<HttpClient>().GetByteArrayAsync("https://files.zirk.eu/Sanara/QuizzTags.txt").GetAwaiter().GetResult());
            string[] lines = File.ReadAllLines("Saves/Game/QuizzTags.txt");
            var preload = new List<BooruQuizzPreloadResult>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] curr = lines[i].Split(' ');
                if (int.Parse(curr[1]) > 3)
                {
                    var booru = new Gelbooru()
                    {
                        HttpClient = provider.GetRequiredService<HttpClient>()
                    };
                    preload.Add(new BooruQuizzPreloadResult(booru, new[] { ".gif", ".png", ".jpg", ".jpeg" }, curr[0], new[] { curr[0] }));
                }
            }
            _preload = preload.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Booru Quizz";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzBooruTags(chan, user, this, settings);

        public string GetRules()
            => "I'll post 3 images, you'll have to give the tag they have in common.";

        public bool IsSafe()
            => false;

        private BooruQuizzPreloadResult[] _preload;
    }
}
