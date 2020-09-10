using Discord;
using SanaraV3.Games.Impl;
using SanaraV3.Games.Preload.Impl.Static;
using SanaraV3.Games.Preload.Result;
using System.Collections.ObjectModel;
using System.Linq;

namespace SanaraV3.Games.Preload.Impl
{
    public sealed class KancollePreload : IPreload
    {
        public KancollePreload()
        {
            _preload = Kancolle.GetShips().Select((x) =>
            {
                return new QuizzPreloadResult(null, new[] { x });
            }).ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "kancolle", "kc", "kantaicollection" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzKancolle(chan, user, this, settings);

        private readonly QuizzPreloadResult[] _preload;
    }
}
