﻿using Discord;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Linq;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class ShiritoriPreload : IPreload
    {
        public ShiritoriPreload()
        {
            _preload = Static.Shiritori.GetWords().ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "shiritori" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Shiritori(chan, user, this, settings);

        public string GetRules()
            => Static.Shiritori.GetRules() +
            "\nWords must be noun, must not end by a ん(n), must not have been already said an must be more than one syllabe.";

        public bool IsSafe()
            => true;

        private readonly ShiritoriPreloadResult[] _preload;
    }
}
