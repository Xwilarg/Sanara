using Discord;
using DiscordUtils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SanaraV3.Subscription
{
    public sealed class SubscriptionManager
    {
        public SubscriptionManager()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Update();
                }
            });
        }

        private async Task Update()
        {
            await Task.Delay(600000); // We check for new content every 10 minutes
            foreach (var sub in _subscriptions)
            {
                try
                {
                    var feed = await sub.GetFeedAsync();
                    if (feed.Length > 0) // If there is anything new in the feed compared to last time
                    {

                    }
                }
                catch (Exception e) // If somehow wrong happens while getting new subscription
                {
                    await Utils.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                }
            }
        }

        private ISubscription[] _subscriptions;
    }
}
