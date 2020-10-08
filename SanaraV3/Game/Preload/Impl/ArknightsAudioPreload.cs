using Discord;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Impl.Static;
using SanaraV3.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Linq;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class ArknightsAudioPreload : IPreload
    {
        public ArknightsAudioPreload()
        {
            _preload = Arknights.GetOperators().Select((x) =>
            {
                return new QuizzPreloadResult("https://aceship.github.io/AN-EN-Tags/etc/voice/" + x.Item1 + "/CN_001.mp3", x.Item2.ToArray());
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

        public string GetRules()
            => "I'll play a vocal line of an operator, you'll have to give his/her name.";

        public bool IsSafe()
            => true;

        private readonly QuizzPreloadResult[] _preload;
    }
}
