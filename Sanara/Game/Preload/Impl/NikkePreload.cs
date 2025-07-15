﻿using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl
{
    public sealed class NikkePreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            _provider = provider;
            var html = provider.GetRequiredService<HttpClient>().GetStringAsync("https://www.prydwen.gg/nikke/characters/").GetAwaiter().GetResult();
            _preload = Regex.Matches(html, "data-src=\"([^\"]+)\" data-srcset=\"[^\"]+\" alt=\"([^\"]+)\"\\/><noscript>").Cast<Match>().Skip(1).Select(x => new QuizzPreloadResult($"https://www.prydwen.gg{x.Groups[1].Value}", new[] { x.Groups[2].Value })).ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Nikke Quizz";

        public AGame CreateGame(CommonMessageChannel chan, CommonUser user, GameSettings settings)
            => new Quizz(_provider, chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of a character, you'll have to give her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
        private IServiceProvider _provider;
    }
}
