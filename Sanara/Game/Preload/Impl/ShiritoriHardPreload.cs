using Discord;
using Sanara.Compatibility;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class ShiritoriHardPreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            _preload = Static.Shiritori.GetWords().Where(x => Shiritori.IsLongEnough(x.Word, 3)).ToArray();
            _provider = provider;
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Shiritori (Hard)";

        public AGame CreateGame(CommonMessageChannel chan, CommonUser user, GameSettings settings)
            => new Shiritori(_provider, chan, user, this, settings, 3);

        public string GetRules()
            => Static.Shiritori.GetRules() +
            "\nWords must be noun, must not end by a ん(n), must not have been already said and must be more than 2 syllabes.";

        public bool IsSafe()
            => true;

        private ShiritoriPreloadResult[] _preload;
        private IServiceProvider _provider;
    }
}
