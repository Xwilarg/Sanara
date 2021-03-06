﻿using Discord;
using SanaraV3.Game.Custom;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Result;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SanaraV3.Game.Preload.Impl
{
    public class CustomPreload : IPreload
    {

        public CustomPreload(CustomGame _game)
        {
            if (_game.Questions == null || _game.Questions.Length == 0)
                throw new ArgumentException("The game must contains at least one question");
            _preload = _game.Questions.Select(x => new QuizzPreloadResult(x.Question, x.Answers)).ToArray();
            _gameName = string.IsNullOrWhiteSpace(_game.Name) ? "Custom Game" : _game.Name;
            _rules = "Custom game:\n" + (string.IsNullOrWhiteSpace(_game.Rules) ? "No rules set" : _game.Rules);
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
               => new[] { _gameName };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings, StaticObjects.ModeText, false);

        public string GetRules()
            => _rules;

        public bool IsSafe()
            => true;

        private readonly QuizzPreloadResult[] _preload;
        private readonly string _gameName;
        private readonly string _rules;
    }
}
