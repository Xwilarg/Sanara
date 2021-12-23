using Discord;
using Sanara.Game.Impl;
using System.Collections.ObjectModel;

namespace Sanara.Game.Preload.Impl
{
    public sealed class BooruFillPreload : IPreload
    {
        public ReadOnlyCollection<IPreloadResult> Load()
            => null;

        public string[] GetGameNames()
            => new[] { "boorufill" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new FillAllBooru(chan, user, this, settings);

        public string GetRules()
            => "I'll post an image, you have to find at least 75% of the tags that are inside.";

        public bool IsSafe()
            => false;
    }
}
