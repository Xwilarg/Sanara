using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Database;
using Sanara.Exception;
using Sanara.Module.Utility;
using Sanara.Subscription.Tags;

namespace Sanara.Module.Command.Impl;

public class Subscription : ISubmodule
{
    public string Name => "Subscription";
    public string Description => "Subscribe to various service and follow them from a channel";

    public CommandData[] GetCommands(IServiceProvider _)
    {
        return new[]
        {
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "subscribe",
                    Description = "Subscribe to a service",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "channel",
                            Description = "Where updates should be posted",
                            Type = ApplicationCommandOptionType.Channel,
                            ChannelTypes = new() { ChannelType.Text },
                            IsRequired = true
                        },
                        new SlashCommandOptionBuilder()
                        {
                            Name = "type",
                            Description = "What you want to be subscribed to",
                            Type = ApplicationCommandOptionType.Integer,
                            IsRequired = true,
                            Choices = new()
                            {
                                new ApplicationCommandOptionChoiceProperties()
                                {
                                    Name = "Anime (SFW)",
                                    Value = (int)SubscriptionType.Anime
                                },
                                new ApplicationCommandOptionChoiceProperties()
                                {
                                    Name = "Inspiration (SFW)",
                                    Value = (int)SubscriptionType.Inspiration
                                }
                            }
                        },
                        new SlashCommandOptionBuilder()
                        {
                            Name = "tags",
                            Description = "Filter upcoming results",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        },
                    },
                    IsNsfw = false
                },
                callback: SubscribeAsync,
                aliases: [],
                adminOnly: true,
                discordSupport: Support.Supported,
                revoltSupport: Support.Unsupported
            )
        };
    }

    public async Task SubscribeAsync(IContext ctx)
    {
        var channel = ctx.GetArgument<ITextChannel>("channel");
        var type = (SubscriptionType)ctx.GetArgument<long>("type");
        var tags = (ctx.GetArgument<string>("tags") ?? "").Split(' ');

        if (type == SubscriptionType.Doujinshi && !channel.IsNsfw)
            throw new CommandFailed("Doujinshi subscription channel must be NSFW");

        await ctx.Provider.GetRequiredService<Db>().SetSubscriptionAsync(channel.GuildId, type switch
        {
            SubscriptionType.Anime => "anime",
            SubscriptionType.Doujinshi => "nhentai",
            SubscriptionType.Inspiration => "inspire",
            _ => throw new NotImplementedException("Invalid subscription type " + type)
        }, channel, type switch
        {
            SubscriptionType.Anime => new DefaultTags(tags, true),
            SubscriptionType.Doujinshi => new NHentaiTags(tags, true),
            SubscriptionType.Inspiration => new DefaultTags(tags, true),
            _ => throw new NotImplementedException("Invalid subscription type " + type)
        });
        await ctx.ReplyAsync($"You subscribed for {type} in {channel.Mention}, use the configure command to remove it\nNew message will be sent either every day or when new content is available, depending of the type of subscription");
    }
}
