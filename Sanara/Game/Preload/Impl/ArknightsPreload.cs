using Discord;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Impl.Static;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class ArknightsPreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            _provider = provider;
            _preload = Arknights.GetOperators().Select((x) =>
            {
                return new QuizzPreloadResult("https://aceship.github.io/AN-EN-Tags/img/characters/" + x.Item1 + "_1.png", x.Item2.ToArray());
            }).ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Arknights Quizz";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(_provider, chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of an operator, you'll have to give his/her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
        private IServiceProvider _provider;
    }
}
