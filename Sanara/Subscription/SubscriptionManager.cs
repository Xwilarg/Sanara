using Discord;
using Discord.Net;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Database;
using Sanara.Subscription.Impl;

namespace Sanara.Subscription;

public sealed class SubscriptionManager
{
    public SubscriptionManager()
    {
        _subscriptions = new ISubscription[]
        {
            // new NHentaiSubscription(),
            new AnimeSubscription(),
            new InspireSubscription()
        };
    }

    public async Task InitAsync(IServiceProvider provider)
    {
        // Init all subscriptions
        foreach (var sub in _subscriptions)
        {
            provider.GetRequiredService<Db>().InitSubscription(sub.GetName());
        }

        // We set the current value on the subscription // TODO: Maybe we shouldn't reset things everytimes the bot start
        foreach (var sub in _subscriptions)
        {

            try
            {
                var currId = provider.GetRequiredService<Db>().GetCurrent(sub.GetName());
                var feed = await sub.GetFeedAsync(provider.GetRequiredService<HttpClient>(), currId, false);
                await provider.GetRequiredService<Db>().SetCurrentAsync(sub.GetName(), feed.Any() ? feed[0].Id : currId); // Somehow doing the GetCurrent inside the GetFeedAsync stuck the bot
            }
            catch (System.Exception e)
            {
                Log.LogErrorAsync(e, null).GetAwaiter().GetResult();
            }
        }

        // Subscription loop
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(600000); // We check for new content every 10 minutes
                await Update(provider);
            }
        });

        _isInit = true;
    }

    public bool IsInit()
        => _isInit;

    public async Task<Dictionary<string, ITextChannel>> GetSubscriptionsAsync(IServiceProvider provider, ulong guildId)
    {
        var d = new Dictionary<string, ITextChannel>();
        foreach (var sub in _subscriptions)
        {
            string name = sub.GetName();
            d.Add(name, await provider.GetRequiredService<Db>().HasSubscriptionExistAsync(guildId, name) ? provider.GetRequiredService<Db>().GetAllSubscriptions(name).Where(x => x.TextChan.GuildId == guildId).FirstOrDefault()?.TextChan : null);
        }
        return d;
    }

    public Dictionary<string, int> GetSubscriptionCount(IServiceProvider provider)
    {
        var d = new Dictionary<string, int>();
        foreach (var sub in _subscriptions)
        {
            string name = sub.GetName();
            d.Add(name, provider.GetRequiredService<Db>().GetAllSubscriptions(name).Length);
        }
        return d;
    }

    private async Task Update(IServiceProvider provider)
    {
        var db = provider.GetRequiredService<Db>();
        var client = provider.GetRequiredService<HttpClient>();
        var isNewDay = await db.CheckForDayUpdateAsync();
        foreach (var sub in _subscriptions)
        {
            try
            {
                var feed = await sub.GetFeedAsync(client, db.GetCurrent(sub.GetName()), isNewDay);
                if (feed.Length > 0) // If there is anything new in the feed compared to last time
                {
                    await db.SetCurrentAsync(sub.GetName(), feed[0].Id);
                    foreach (var elem in db.GetAllSubscriptions(sub.GetName()))
                    {
                        try
                        {
                            // Subscription that works daily need to remove the previous message
                            if (sub.DeleteOldMessage)
                            {
                                var lastMsg = await elem.TextChan.GetMessagesAsync(1).FlattenAsync();
                                if (lastMsg.Any() && lastMsg.ElementAt(0).Author.Id == Program.ClientId)
                                {
                                    await lastMsg.ElementAt(0).DeleteAsync();
                                }
                            }
                            foreach (var data in feed)
                            {
                                if (elem.Tags.IsTagValid(data.Tags)) // Check if tags are valid with black/whitelist
                                {
                                    await elem.TextChan.SendMessageAsync(embed: data.Embed);
                                }
                            }
                        }
                        catch (HttpException http)
                        {
                            if (!http.DiscordCode.HasValue ||
                                (http.DiscordCode.Value != DiscordErrorCode.MissingPermissions && http.DiscordCode.Value != DiscordErrorCode.UnknownChannel))
                                throw;
                        }
                        catch (System.Exception e)
                        {
                            await Log.LogErrorAsync(e, null);
                        }
                    }
                }
            }
            catch (System.Exception e) // If somehow wrong happens while getting new subscription
            {
                await Log.LogErrorAsync(e, null);
            }
        }
    }

    private readonly ISubscription[] _subscriptions;
    private bool _isInit = false;
}
