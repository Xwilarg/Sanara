using Discord;
using Sanara.Compatibility;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class ShiritoriPreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            _preload = Static.Shiritori.GetWords().ToArray();
            _provider = provider;
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Shiritori";

        public AGame CreateGame(IMessageChannel chan, CommonUser user, GameSettings settings)
            => new Shiritori(_provider, chan, user, this, settings);

        public string GetRules()
            => Static.Shiritori.GetRules() +
            "\nWords must be noun, must not end by a ん(n), must not have been already said and must be more than one syllabe.";

        public bool IsSafe()
            => true;

        private ShiritoriPreloadResult[] _preload;
        private IServiceProvider _provider;
    }
}
