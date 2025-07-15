using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using Sanara.Database;
using Sanara.Subscription;

namespace Sanara.Module.Utility;

public class Settings
{
    public static async Task<(CommonEmbedBuilder Embed, MessageComponent Components)> GetSettingsDisplayAsync(IServiceProvider provider, IGuild guild)
    {
        var subs = await provider.GetRequiredService<SubscriptionManager>().GetSubscriptionsAsync(provider, guild.Id);
        var mySubs = subs?.Select(x => $"**{Utils.ToWordCase(x.Key)}**: {(x.Value == null ? "None" : x.Value.Mention)}");
        var button = new ComponentBuilder();
        if (subs != null)
        {
            foreach (var sub in subs)
            {
                if (sub.Value != null)
                {
                    button.WithButton($"Remove {sub.Key} subscription", $"delSub-{sub.Key}", style: ButtonStyle.Danger);
                }
            }
        }
        button.WithButton("Database dump", "dump", style: ButtonStyle.Secondary);
        button.WithButton("Toggle translation from flags", "flag", style: ButtonStyle.Secondary);
        var embed = new CommonEmbedBuilder
        {
            Title = guild.ToString(),
            Color = Color.Purple
        };
        embed.AddField("Translation from flags", provider.GetRequiredService<Db>().GetGuild(guild.Id).TranslateUsingFlags ? "Enabled" : "Disabled");
        embed.AddField("Subscriptions", subs == null ? "Not yet initialized" : (mySubs.Any() ? string.Join("\n", mySubs) : "None"));
        return (embed, button.Build());
    }
}
