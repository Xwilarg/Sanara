using System.Collections.Generic;

namespace SanaraV3.Subscription
{
    public abstract class ASubscriptionTags
    {
        public abstract Dictionary<string, string[]> GetDefaultBlacklist();
    }
}
