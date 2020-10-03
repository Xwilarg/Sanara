using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordUtils;
using SanaraV3.Exception;
using SharpRaven.Data;
using System.Threading.Tasks;

namespace SanaraV3
{
    public static class Log
    {
        public static async Task ErrorAsync(LogMessage msg)
        {
            // TODO: Manage availability

            await Utils.Log(msg); // First thing, we log the error in the console
            if (msg.Exception is CommandException ce) //Eexception thrown in a Discord channel
            {
                if (msg.Exception.InnerException is CommandFailed || msg.Exception.InnerException is NotYetAvailable)
                {
                    await ce.Context.Channel.SendMessageAsync(msg.Exception.InnerException.Message);
                    return;
                }

                if (StaticObjects.RavenClient != null) // If we can log the error to Sentry
                    StaticObjects.RavenClient.Capture(new SentryEvent(new System.Exception(ce.Context.Message.ToString(), msg.Exception)));

                var sentMsg = await ce.Context.Channel.SendMessageAsync(embed: new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "An error occured",
                    Description = "The error was automatically reported. If the error persist, please contact the bot owner.",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Add a 🕷️ emote to have more information about the error"
                    }
                }.Build());

                StaticObjects.Errors.Add(sentMsg.Id, new ErrorData(ce.Context.Message.CreatedAt.UtcDateTime, ce));

                await sentMsg.AddReactionAsync(new Emoji("🕷"));

                StaticObjects.Website?.AddErrorAsync(ce.InnerException);
            }
            else
            {
                if (StaticObjects.RavenClient != null)
                    StaticObjects.RavenClient.Capture(new SentryEvent(msg.Exception));

                StaticObjects.Website?.AddErrorAsync(msg.Exception);
            }
        }

        /// <summary>
        /// Callback handing when the user add a little spider to have more info about a bug
        /// </summary>
        public static async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel chan, SocketReaction react)
        {
            string emote = react.Emote.ToString();
            // If emote is not from the bot and is an arrow emote
            if (react.User.Value.Id != StaticObjects.ClientId && emote == "🕷" && StaticObjects.Errors.ContainsKey(msg.Id))
            {
                var error = StaticObjects.Errors[msg.Id];
                await (await msg.GetOrDownloadAsync()).ModifyAsync((curr) =>
                {
                    curr.Embed = new EmbedBuilder
                    {
                        Color = Color.Red,
                        Title = error.Exception.InnerException.GetType().ToString(),
                        Description = error.Exception.InnerException.Message,
                        Footer = new EmbedFooterBuilder
                        {
                            Text = "Command used: " + ((CommandException)error.Exception).Context.Message
                        }
                    }.Build();
                });
                StaticObjects.Errors.Remove(msg.Id);
            }
        }
    }
}
