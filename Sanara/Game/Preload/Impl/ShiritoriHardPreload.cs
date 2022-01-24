using Discord;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class ShiritoriHardPreload : IPreload
    {
        public void Init()
        {
            _preload = Static.Shiritori.GetWords().Where(x => Shiritori.IsLongEnough(x.Word, 3)).ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Shiritori (Hard)";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Shiritori(chan, user, this, settings, 3);

        public string GetRules()
            => Static.Shiritori.GetRules() +
            "\nWords must be noun, must not end by a ん(n), must not have been already said and must be more than 2 syllabes.";

        public bool IsSafe()
            => true;

        private ShiritoriPreloadResult[] _preload;
    }
}
