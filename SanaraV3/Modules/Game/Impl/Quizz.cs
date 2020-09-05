using Discord;
using SanaraV3.Exceptions;
using SanaraV3.Modules.Game.PostMode;
using SanaraV3.Modules.Game.Preload;
using SanaraV3.Modules.Game.Preload.Result;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Game.Impl
{
    public class Quizz : AGame
    {
        /// <summary>
        /// Called by QuizzAudio
        /// </summary>
        protected Quizz(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings, IPostMode mode) : base(textChan, user, mode, settings)
        {
            _words = new List<QuizzPreloadResult>(preload.Load().Cast<QuizzPreloadResult>());
            _allValidNames = _words.SelectMany(x => x.Answers).ToArray();
        }

        public Quizz(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, StaticObjects.ModeUrl, settings)
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
            if (!_allValidNames.Contains(answer.ToUpper()))
                throw new InvalidGameAnswer(""); // We just add a reaction to the message to not spam the text channel
            if (!_current.Answers.Contains(answer.ToUpper()))
                throw new InvalidGameAnswer("No this is not " + answer + ".");
            return Task.CompletedTask;
        }

        protected override string GetAnswer()
        {
            string name = _current.Answers[0];
            return $"The right answer was {name[0] + string.Join("", name.Skip(1)).ToLower()}.";
        }

        protected override int GetGameTime()
            => 15;

        protected override string GetRules()
            => "I'll post an image of a character, you'll have to give his name.";

        protected override string GetSuccessMessage()
            => "Congratulations, you found the right answer.";

        private QuizzPreloadResult _current; // Word to guess
        private List<QuizzPreloadResult> _words;
        private readonly string[] _allValidNames;
    }
}
