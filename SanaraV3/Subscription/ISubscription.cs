using System.Threading.Tasks;

namespace SanaraV3.Subscription
{
    public interface ISubscription
    {
        public abstract Task<FeedItem[]> GetFeedAsync();

        public void SetCurrent(int current);

        public int GetCurrent();
    }
}
