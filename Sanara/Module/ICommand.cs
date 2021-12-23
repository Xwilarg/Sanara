using Discord;
using Sanara.Help;

namespace Sanara.Module
{
    public interface ICommand
    {
        public SubmoduleInfo Help();
        public SlashCommandProperties[] CreateCommands();
    }
}
