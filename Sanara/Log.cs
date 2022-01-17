﻿using Discord;
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
            Console.Out.WriteLineAsync(msg.ToString());
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }

        public static async Task LogErrorAsync(System.Exception e, IDiscordInteraction? ctx)
        {
            await LogAsync(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));

            if (ctx != null)
            {
                var button = new ComponentBuilder()
                        .WithButton("More information", $"error-{ctx.Id}");

                StaticObjects.Errors.Add($"error-{ctx.Id}", e);
                var embed = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "An error occured",
                    Description = "The error was automatically reported. If the error persist, please contact the bot owner."
                }.Build();

                if (ctx.HasResponded)
                {
                    await ctx.ModifyOriginalResponseAsync(x =>
                    {
                        x.Embed = embed;
                        x.Components = button.Build();
                    });
                }
                else
                {
                    await ctx.RespondAsync(embed: embed, components: button.Build());
                }

                if (SentrySdk.IsEnabled)
                    SentrySdk.CaptureException(new System.Exception($"Error while processing {ctx}", e));
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
