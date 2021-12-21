namespace Sanara.Subscription.Tags
{
    public sealed class AnimeTags : ASubscriptionTags
    {
        public AnimeTags(string[] tags, bool addDefaultTags) : base(tags, addDefaultTags)
        { }

        public override Dictionary<string, string[]> GetDefaultBlacklist()
        {
            return new();
        }
    }
}
