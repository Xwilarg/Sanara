using Discord;
using Discord.WebSocket;
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
            Console.WriteLine(msg);
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }

        public static async Task LogErrorAsync(System.Exception e, SocketSlashCommand? ctx)
        {
            await LogAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));

            if (ctx != null)
            {
                var button = new ComponentBuilder()
                        .WithButton("More information", ctx.Id.ToString());

                StaticObjects.Errors.Add(ctx.Id.ToString(), e);

                await ctx.RespondAsync(embed: new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "An error occured",
                    Description = "The error was automatically reported. If the error persist, please contact the bot owner."
                }.Build(), components: button.Build());

                if (SentrySdk.IsEnabled)
                    SentrySdk.CaptureException(new System.Exception($"Error while processing {ctx}", e));
            }
            else
            {
                if (SentrySdk.IsEnabled)
                    SentrySdk.CaptureException(e);
            }

            StaticObjects.Website?.AddErrorAsync(e);
        }
    }
}
