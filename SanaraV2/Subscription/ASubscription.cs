using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV2.Subscription
{
    public abstract class ASubscription
    {
        public abstract Task<(int, EmbedBuilder)[]> GetFeed();

        public async Task UpdateChannelAsync(List<(ITextChannel, SubscriptionTags)> subscriptions)
        {
            var data = await GetFeed();
            if (data.Length > 0)
            {
                Current = data[0].Item1;
                for (int i = subscriptions.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        foreach (var elem in data)
                        {
                            await subscriptions[i].Item1.SendMessageAsync("", false, elem.Item2.Build());
                        }
                    }
                    catch (Exception e)
                    {
                        await Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                    }
                }
            }
        }

        public int Current { set; get; }
    }
}
