using Discord;

namespace Sanara.Module.Command;

public record CommandData(SlashCommandBuilder slashCommand, Func<IContext, Task> callback, string[] aliases, Support revoltSupport = Support.Unsupported, Support discordSupport = Support.Supported, bool adminOnly = false)
{
    /// <summary>
    /// Slash command info
    /// </summary>
    public SlashCommandBuilder SlashCommand { get; } = slashCommand;
    /// <summary>
    /// Method to be called to execute the command
    /// </summary>
    public Func<IContext, Task> Callback { get; } = callback;
    /// <summary>
    /// Other names that can be used instead of the command name
    /// </summary>
    public string[] Aliases { get; } = aliases.Select(x => x.ToUpperInvariant()).ToArray();
    public bool IsAdminOnly { get; } = adminOnly;

    public Support DiscordSupport { get; }
    public Support RevoltSupport { get; }
}

public enum Support
{
    Supported,
    Partial,
    Unsupported
}