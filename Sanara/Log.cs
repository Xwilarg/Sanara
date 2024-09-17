using Discord;
using Sanara.Database;
using Sanara.Module.Command;
using System.Net;

namespace Sanara
{
    public static class Log
    {
        public static void Init(Db db)
        {
            _db = db;
        }

        private static Db _db;
        public static Dictionary<string, System.Exception> Errors { get; } = [];

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

        public static async Task LogErrorAsync(System.Exception e, IContext ctx)
        {
            await LogAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));

            if (ctx != null)
            {
                try
                {
                    if (e is HttpRequestException hre && hre.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        await ctx.ReplyAsync(embed: new EmbedBuilder
                        {
                            Color = Color.Orange,
                            Title = "Service Unavailable",
                            Description = "The command failed with an error 503, this probably mean the website used for this service is unavailable"
                        }.Build());
                    }
                    else
                    {
                        var id = Guid.NewGuid();
                        var button = new ComponentBuilder()
                                .WithButton("More information", $"error-{id}");

                        Errors.Add($"error-{id}", e);
                        var embed = new EmbedBuilder
                        {
                            Color = Color.Red,
                            Title = "An error occured",
                            Description = "The error was automatically reported, you can click the button below to have more information about it"
                        }.Build();

                        await ctx.ReplyAsync(embed: embed, components: button.Build());
                    }
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

            if (_db != null) await _db.AddErrorAsync(e);
        }
    }
}
