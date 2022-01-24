using BooruSharp.Booru;
using Discord;
using Sanara.Exception;
using Sanara.Game.Preload;
using Sanara.Game.Preload.Result;
using Sanara.Module.Command;

namespace Sanara.Game.Impl
{
    public abstract class QuizzBooru : Quizz
    {
        public QuizzBooru(IMessageChannel textChan, IUser user, IPreload preload, GameSettings settings) : base(textChan, user, preload, settings)
        {
            var info = new List<BooruQuizzPreloadResult>(preload.Load().Cast<BooruQuizzPreloadResult>())[0];
            _booru = info.Booru;
            _allowedFormats = info.AllowedFormats;
        }

        protected override Task CheckAnswerInternalAsync(ICommandContext answer)
        {
            string userAnswer = answer.GetArgument<string>("answer");
            if (!_current.Answers.Any(x => Utils.EasyCompare(x, userAnswer)))
                throw new InvalidGameAnswer("");
            return Task.CompletedTask;
        }

        protected override int GetGameTime()
            => 30;

        protected override string GetHelp()
        {
            var answer = _current.Answers[0].Replace('_', ' ');
            string answerHelp = char.ToUpper(answer[0]).ToString();
            foreach (var c in answer.Skip(1))
            {
                if (c == ' ')
                    answerHelp += c;
                else
                    answerHelp += "\\*";
            }
            return answerHelp;
        }

        protected ABooru _booru;
        protected string[] _allowedFormats;
    }
}
