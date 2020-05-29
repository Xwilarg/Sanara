using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV2.Subscription
{
    public abstract class ASubscription
    {
        public abstract Task<(int, EmbedBuilder, string[])[]> GetFeed();

        public async Task UpdateChannelAsync(List<(ITextChannel, SubscriptionTags)> subscriptions)
        {
            var data = await GetFeed();
            if (data.Length > 0)
            {
                await SetCurrent(data[0].Item1);
                for (int i = subscriptions.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        foreach (var elem in data)
                        {
                            var sub = subscriptions[i];
                            if (sub.Item2 != null)
                            {
                                if (!sub.Item2.IsTagValid(elem.Item3))
                                    continue;
                            }
                            await sub.Item1.SendMessageAsync("", false, elem.Item2.Build());
                        }
                    }
                    catch (Exception e)
                    {
                        await Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                    }
                }
            }
        }

        public abstract Task SetCurrent(int value);

        public abstract int GetCurrent();
    }
}
