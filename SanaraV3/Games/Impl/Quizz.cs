﻿using Discord;
using DiscordUtils;
using SanaraV3.Exceptions;
using SanaraV3.Games.PostMode;
using SanaraV3.Games.Preload;
using SanaraV3.Games.Preload.Result;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Games.Impl
{
    /// <summary>
    /// Basic quizz game
    /// </summary>
    public class Quizz : AGame
    {
        /// <summary>
        /// Called by QuizzAudio
        /// </summary>
        protected Quizz(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings, IPostMode mode) : base(textChan, user, preload, mode, settings)
        {
            _words = new List<QuizzPreloadResult>(preload.Load().Cast<QuizzPreloadResult>());
            _allValidNames = _words.SelectMany(x => x.Answers).ToArray();
        }

        public Quizz(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, preload, StaticObjects.ModeUrl, settings)
        {
            _words = new List<QuizzPreloadResult>(preload.Load().Cast<QuizzPreloadResult>());
            _allValidNames = _words.SelectMany(x => x.Answers).ToArray();
        }

        protected override string GetPostInternal()
        {
            if (_words.Count == 0)
                throw new GameLost("All characters were found! Congratulations!");

            _current = _words[StaticObjects.Random.Next(0, _words.Count)];
            _words.Remove(_current);
            return _current.ImageUrl;
        }

        protected override Task CheckAnswerInternalAsync(string answer)
        {
            string userAnswer = Utils.CleanWord(answer);
            if (!_allValidNames.Any(x => Utils.CleanWord(x) == userAnswer))
                throw new InvalidGameAnswer(""); // We just add a reaction to the message to not spam the text channel
            if (!_current.Answers.Any(x => Utils.CleanWord(x) == userAnswer))
                throw new InvalidGameAnswer("No this is not " + answer + ".");
            return Task.CompletedTask;
        }

        protected override string GetAnswer()
        {
            string name = _current.Answers[0].Replace("_", " "); // Clean the answer a bit for game that didn't do it
            return $"The right answer was {name}.";
        }

        protected override int GetGameTime()
            => 15;

        protected override string GetRules()
            => "I'll post an image of a character, you'll have to give his name.";

        protected override string GetSuccessMessage()
            => "Congratulations, you found the right answer." + ((_score + 1) % 10 == 0 ? $"\nAlready {_score + 1} out of {_allValidNames.Length} found!" : "");

        protected QuizzPreloadResult _current; // Word to guess
        protected List<QuizzPreloadResult> _words;
        protected readonly string[] _allValidNames;
    }
}