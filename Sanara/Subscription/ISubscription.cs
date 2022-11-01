namespace Sanara.Subscription
{
    public interface ISubscription
    {
        public abstract Task<FeedItem[]> GetFeedAsync(int current, bool isNewDay); // Get the subscription feed

        public string GetName();

        public bool DeleteOldMessage { get; }
    }
}
