using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Database;
using Sanara.Module.Command;
using Sanara.Subscription;

namespace Sanara.Module.Button
{
    public class Settings
    {
        public static async Task DatabaseDump(IContext ctx)
        {
            await ctx.ReplyAsync("```json\n" +
                await ctx.Provider.GetRequiredService<Db>().DumpAsync(((ITextChannel)ctx.Channel).Guild.Id) +
                "\n```", ephemeral: true);
        }

        public static async Task RemoveSubscription(IContext ctx, string key)
        {
            var guildId = ((ITextChannel)ctx.Channel).Guild.Id;
            var subs = await ctx.Provider.GetRequiredService<SubscriptionManager>().GetSubscriptionsAsync(ctx.Provider, guildId);
            if (subs == null)
            {
                await ctx.ReplyAsync("Subscription system is not ready yet", ephemeral: true);
            }
            else if (subs[key] == null)
            {
                await ctx.ReplyAsync("This subscription was already removed", ephemeral: true);
            }
            else
            {
                await ctx.Provider.GetRequiredService<Db>().RemoveSubscriptionAsync(guildId, key);
                await ctx.ReplyAsync("The subscription was removed", ephemeral: true);
            }
        }
    }
}
