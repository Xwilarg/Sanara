namespace Sanara.Subscription.Tags
{
    public sealed class DefaultTags : ASubscriptionTags
    {
        public DefaultTags(string[] tags, bool addDefaultTags) : base(tags, addDefaultTags)
        { }

        public override Dictionary<string, string[]> GetDefaultBlacklist()
        {
            return new();
        }
    }
}
