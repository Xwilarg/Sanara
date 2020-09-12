using Discord;
using DiscordUtils;
using SanaraV3.Subscription.Impl;
using System;
using System.Threading.Tasks;

namespace SanaraV3.Subscription
{
    public sealed class SubscriptionManager
    {
        public SubscriptionManager()
        {
            _subscriptions = new[]
            {
                new NHentaiSubscription()
            };
        }

        public async Task InitAsync()
        {
            // Init all subscriptions
            foreach (var sub in _subscriptions)
                StaticObjects.Db.InitSubscription(sub.GetName());

            // We set the current value on the subscription // TODO: Maybe we shouldn't reset things everytimes the bot start
            foreach (var sub in _subscriptions)
                await StaticObjects.Db.SetCurrentAsync(sub.GetName(), (await sub.GetFeedAsync(StaticObjects.Db.GetCurrent(sub.GetName())))[0].Id); // Somehow doing the GetCurrent inside the GetFeedAsync stuck the bot

            // Subscription loop
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(600000); // We check for new content every 10 minutes
                    await Update();
                }
            });
        }

        private async Task Update()
        {
            foreach (var sub in _subscriptions)
            {
                try
                {
                    var feed = await sub.GetFeedAsync(StaticObjects.Db.GetCurrent(sub.GetName()));
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
