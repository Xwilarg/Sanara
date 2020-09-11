using System.Threading.Tasks;

namespace SanaraV3.Subscription
{
    public interface ISubscription
    {
        public abstract Task<FeedItem[]> GetFeedAsync(); // Get the subscription feed

        public string GetName();
    }
}
