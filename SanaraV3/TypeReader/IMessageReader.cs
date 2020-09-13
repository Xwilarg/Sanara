using Discord;
using Discord.Commands;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV3.TypeReader
{
    public sealed class IMessageReader : Discord.Commands.TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (ulong.TryParse(input, out ulong result))
            {
                var msg = await context.Channel.GetMessageAsync(result);
                if (msg == null)
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "No message was found in this channel for this id.");
                return TypeReaderResult.FromSuccess(msg);
            }
            var match = Regex.Match(input, "https:\\/\\/([^\\.]+\\.)?discordapp.com\\/channels\\/([0-9]{18})\\/([0-9]{18})\\/([0-9]{18})");
            if (match.Success && context.Channel is ITextChannel && match.Groups[2].Value == context.Guild.Id.ToString())
            {
                var msg = await (await context.Guild.GetTextChannelAsync(ulong.Parse(match.Groups[3].Value)))?.GetMessageAsync(ulong.Parse(match.Groups[4].Value));
                if (msg == null)
                    return TypeReaderResult.FromError(CommandError.ParseFailed, "No message was found in this guild from this url.");
                return TypeReaderResult.FromSuccess(msg);
            }
            return TypeReaderResult.FromError(CommandError.ParseFailed, "Can't convert string to IMessage.");
        }
    }
}
