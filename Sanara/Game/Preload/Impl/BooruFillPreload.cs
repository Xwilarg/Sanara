﻿using Discord;
using Sanara.Compatibility;
using Sanara.Game.Impl;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class BooruFillPreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            _provider = provider;
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => null;

        public string Name => "Booru Fill";

        public AGame CreateGame(CommonMessageChannel chan, CommonUser user, GameSettings settings)
            => new FillAllBooru(_provider, chan, user, this, settings);

        public string GetRules()
            => "I'll post an image, you have to find at least 75% of the tags that are inside.";

        public bool IsSafe()
            => false;

        private IServiceProvider _provider;
    }
}
