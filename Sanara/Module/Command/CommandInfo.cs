using Discord;

namespace Sanara.Module.Command
{
    public record CommandInfo
    {
        public CommandInfo(SlashCommandProperties slashCommand, Func<ICommandContext, Task> callback, Precondition precondition, string[] aliases, bool needDefer)
            => (SlashCommand, Callback, Precondition, Aliases, NeedDefer) = (slashCommand, callback, precondition, aliases.Select(x => x.ToUpperInvariant()).ToArray(), needDefer);

        /// <summary>
        /// Slash command info
        /// </summary>
        public SlashCommandProperties SlashCommand { private init; get; }
        /// <summary>
        /// Method to be called to execute the command
        /// </summary>
        public Func<ICommandContext, Task> Callback { private init; get; }
        /// <summary>
        /// Requirements for the command to be launched
        /// </summary>
        public Precondition Precondition { private init; get; }
        /// <summary>
        /// Does the command need to be defer
        /// aka: may it takes more than 2 sec to run
        /// </summary>
        public bool NeedDefer { private set; get; }
        /// <summary>
        /// Other names that can be used instead of the command name
        /// </summary>
        public string[] Aliases { private init; get; }
    }
}
