using Discord;
using SanaraV3.Modules.Game.Impl;
using SanaraV3.Modules.Game.Preload.Impl.Static;
using SanaraV3.Modules.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Linq;

namespace SanaraV3.Modules.Game.Preload.Impl
{
    public sealed class ArknightsAudioPreload : IPreload
    {
        public ArknightsAudioPreload()
        {
            _preload = Arknights.GetOperators().Select((x) =>
            {
                return new QuizzPreloadResult("https://aceship.github.io/AN-EN-Tags/etc/voice/" + x.Item1 + "/CN_042.mp3", x.Item2.ToArray());
            }).ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "arknights", "ak" };

        public string GetNameArg()
            => "audio";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzAudio(chan, user, this, settings);

        private readonly QuizzPreloadResult[] _preload;
    }
}
