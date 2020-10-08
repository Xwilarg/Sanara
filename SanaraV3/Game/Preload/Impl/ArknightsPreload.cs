using Discord;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Impl.Static;
using SanaraV3.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Linq;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class ArknightsPreload : IPreload
    {
        public ArknightsPreload()
        {
            _preload = Arknights.GetOperators().Select((x) =>
            {
                return new QuizzPreloadResult("https://aceship.github.io/AN-EN-Tags/img/characters/" + x.Item1 + "_1.png", x.Item2.ToArray());
            }).ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "arknights", "ak" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of an operator, you'll have to give his/her name.";

        public bool IsSafe()
            => true;

        private readonly QuizzPreloadResult[] _preload;
    }
}
