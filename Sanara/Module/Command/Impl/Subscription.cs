using Discord;
using Discord.WebSocket;
using Sanara.Help;
using Sanara.Module.Utility;
using Sanara.Subscription.Tags;

namespace Sanara.Module.Command.Impl
{
    public class Subscription : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Subscription", "Subscribe to various service and follow them from a channel");
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
                new CommandInfo(
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
#if NSFW_BUILD
                                    new ApplicationCommandOptionChoiceProperties()
                                    {
                                        Name = "Doujinshi (NSFW)",
                                        Value = (int)SubscriptionType.Doujinshi
                                    }
#endif
                                }
                            },
                            new SlashCommandOptionBuilder()
                            {
                                Name = "tags",
                                Description = "Filter upcoming results",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
                            },
                        }
                    }.Build(),
                    callback: SubscribeAsync,
                    precondition: Precondition.GuildOnly | Precondition.AdminOnly,
                    needDefer: false
                )
            };
        }

        public async Task SubscribeAsync(SocketSlashCommand ctx)
        {
            var channel = (ITextChannel)ctx.Data.Options.First(x => x.Name == "channel").Value;
            var type = (SubscriptionType)(long)ctx.Data.Options.First(x => x.Name == "type").Value;
            var tags = ((string)(ctx.Data.Options.FirstOrDefault(x => x.Name == "tags")?.Value ?? "")).Split(' ');

            await StaticObjects.Db.SetSubscriptionAsync(channel.GuildId, type switch
            {
                SubscriptionType.Anime => "anime",
                _ => throw new NotImplementedException("Invalid subscription type " + type)
            }, channel, type switch
            {
                SubscriptionType.Anime => new AnimeTags(tags, true),
                _ => throw new NotImplementedException("Invalid subscription type " + type)
            });
            await ctx.RespondAsync($"You subscribed for {type} to {channel.Mention}.");
        }
    }
}
