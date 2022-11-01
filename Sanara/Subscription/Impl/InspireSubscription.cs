using Discord;
using Sanara.Module.Utility;

namespace Sanara.Subscription.Impl
{
    public class InspireSubscription : ISubscription
    {
        public bool DeleteOldMessage => true;

        public async Task<FeedItem[]> GetFeedAsync(int current, bool isNewDay)
        {
            if (!isNewDay)
            {
                var inspire = await Inspire.GetInspireAsync();
                return new[] { new FeedItem(inspire.GetHashCode(), new EmbedBuilder()
                {
                    Color = Color.Blue,
                    ImageUrl = await Inspire.GetInspireAsync()
                }.Build(), Array.Empty<string>()) };
            }
            return Array.Empty<FeedItem>();
        }

        public string GetName() => "inspire";
    }
}
