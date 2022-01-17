using Discord;
using Discord.WebSocket;

namespace Sanara.Module.Button
{
    public class Settings
    {
        public static async Task DatabaseDump(SocketMessageComponent ctx)
        {
            await ctx.RespondAsync("```json\n" +
                await StaticObjects.Db.DumpAsync(((ITextChannel)ctx.Channel).Guild.Id) +
                "\n```", ephemeral: true);
        }

        public static async Task RemoveSubscription(SocketMessageComponent ctx, string key)
        {
            var guildId = ((ITextChannel)ctx.Channel).Guild.Id;
            var subs = await StaticObjects.GetSubscriptionsAsync(guildId);
            if (subs == null)
            {
                await ctx.RespondAsync("Subscription system is not ready yet", ephemeral: true);
            }
            else if (subs[key] == null)
            {
                await ctx.RespondAsync("This subscription was already removed", ephemeral: true);
            }
            else
            {
                await StaticObjects.Db.RemoveSubscriptionAsync(, key);
                await ctx.RespondAsync($"{ctx.User.Mention} removed a subscription");
            }
        }
    }
}
