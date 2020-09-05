using Discord;
using SanaraV3.Modules.Game.Impl;
using SanaraV3.Modules.Game.Preload.Impl.Static;
using SanaraV3.Modules.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Linq;

namespace SanaraV3.Modules.Game.Preload.Impl
{
    public sealed class KancolleAudioPreload : IPreload
    {
        public KancolleAudioPreload()
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
            => "audio";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzAudioKancolle(chan, user, this, settings);

        private readonly QuizzPreloadResult[] _preload;
    }
}
