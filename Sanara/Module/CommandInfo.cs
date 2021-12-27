using Discord;
using Discord.WebSocket;

namespace Sanara.Module
{
    public record CommandInfo
    {
        public CommandInfo(SlashCommandProperties slashCommand, Func<SocketSlashCommand, Task> callback, Precondition precondition)
            => (SlashCommand, Callback, Precondition) = (slashCommand, callback, precondition);

        public SlashCommandProperties SlashCommand { private init; get; }
        public Func<SocketSlashCommand, Task> Callback { private init; get; }
        public Precondition Precondition { private init; get; }
    }
}
