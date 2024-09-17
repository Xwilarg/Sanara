using BooruSharp.Booru;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class AnimePreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            if (!File.Exists("Saves/Game/QuizzAnime.txt"))
                File.WriteAllBytes("Saves/Game/QuizzAnime.txt", provider.GetRequiredService<HttpClient>().GetByteArrayAsync("https://files.zirk.eu/Sanara/QuizzAnime.txt").GetAwaiter().GetResult());
            string[] lines = File.ReadAllLines("Saves/Game/QuizzAnime.txt");
            var preload = new List<BooruQuizzPreloadResult>();
            for (int i = 0; i < lines.Length; i++)
            {
                string[] curr = lines[i].Split(' ');
                if (int.Parse(curr[1]) > 10)
                {
                    var booru = new Sakugabooru()
                    {
                        HttpClient = provider.GetRequiredService<HttpClient>()
                    };
                    preload.Add(new BooruQuizzPreloadResult(booru, new[] { ".mp4", ".webm" }, curr[0], new[] { curr[0] }));
                }
            }
            _preload = preload.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Anime Quizz";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzBooruAnime(chan, user, this, settings);

        public string GetRules()
            => "I'll post an extract from an anime, you'll have to give its name.";

        public bool IsSafe()
            => true;

        private BooruQuizzPreloadResult[] _preload;
    }
}
