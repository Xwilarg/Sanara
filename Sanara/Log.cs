using Discord;
using Discord.WebSocket;
using Sanara.Module.Command;
using Sentry;

namespace Sanara
{
    public static class Log
    {
        public static Task LogAsync(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            Console.ForegroundColor = msg.Severity switch
            {
                LogSeverity.Critical => ConsoleColor.DarkRed,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.DarkYellow,
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Verbose => ConsoleColor.Green,
                LogSeverity.Debug => ConsoleColor.DarkGreen,
                _ => throw new NotImplementedException("Invalid log level " + msg.Severity)
            };
            Console.Out.WriteLineAsync(msg.ToString());
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }

        public static async Task LogErrorAsync(System.Exception e, ICommandContext ctx)
        {
            await LogAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));

            if (ctx != null)
            {
                try
                {
                    var id = Guid.NewGuid();
                    var button = new ComponentBuilder()
                            .WithButton("More information", $"error-{id}");

                    StaticObjects.Errors.Add($"error-{id}", e);
                    var embed = new EmbedBuilder
                    {
                        Color = Color.Red,
                        Title = "An error occured",
                        Description = "The error was automatically reported. If the error persist, please contact the bot owner."
                    }.Build();

                    await ctx.ReplyAsync(embed: embed, components: button.Build());
                }
                catch (System.Exception ex)
                {
                    // TODO: Couldn't send error in channel
                }


                if (SentrySdk.IsEnabled)
                    SentrySdk.CaptureException(new System.Exception($"Command {ctx} failed", e));
            }
            else
            {
                if (SentrySdk.IsEnabled)
                    SentrySdk.CaptureException(e);
            }

            await StaticObjects.Db.AddErrorAsync(e);
        }
    }
}
