using Discord;
using Discord.WebSocket;

namespace Sanara.Module
{
    public record CommandInfo
    {
        public CommandInfo(SlashCommandProperties slashCommand, Func<SocketSlashCommand, Task> callback)
            => (SlashCommand, Callback) = (slashCommand, callback);

        public SlashCommandProperties SlashCommand { private init; get; }
        public Func<SocketSlashCommand, Task> Callback { private init; get; }
    }
}
