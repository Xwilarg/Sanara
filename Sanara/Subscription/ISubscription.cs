namespace Sanara.Subscription
{
    public interface ISubscription
    {
        public abstract Task<FeedItem[]> GetFeedAsync(HttpClient client, int current, bool isNewDay); // Get the subscription feed

        public string GetName();

        public bool DeleteOldMessage { get; }
    }
}
