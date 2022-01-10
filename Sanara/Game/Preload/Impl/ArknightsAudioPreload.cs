﻿using Discord;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Impl;
using Sanara.Game.Preload.Impl.Static;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class ArknightsAudioPreload : IPreload
    {
        public void Init()
        {
            _preload = Arknights.GetOperators().Select((x) =>
            {
                return new QuizzPreloadResult("https://aceship.github.io/AN-EN-Tags/etc/voice/" + x.Item1 + "/CN_001.mp3", x.Item2.ToArray());
            }).ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Arknights Audio Quizz";
        public string Description => "Find the name of an Arknights character from a voice clip";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzAudio(chan, user, this, settings);

        public string GetRules()
            => "I'll play a vocal line of an operator, you'll have to give his/her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
    }
}
