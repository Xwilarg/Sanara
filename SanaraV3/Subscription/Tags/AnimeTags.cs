using System.Collections.Generic;

namespace SanaraV3.Subscription.Tags
{
    public sealed class AnimeTags : ASubscriptionTags
    {
        public AnimeTags(string[] tags, bool addDefaultTags) : base(tags, addDefaultTags)
        { }

        public override Dictionary<string, string[]> GetDefaultBlacklist()
        {
            return new Dictionary<string, string[]>();
        }
    }
}
