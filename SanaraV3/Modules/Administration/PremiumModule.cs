using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Administration
{
    public sealed class PremiumModule : ModuleBase, IModule
    {
        public string ModuleName { get { return "Premium"; } }

        [Command("Premium")]
        public async Task PremiumAsync()
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "Premium",
                Color = Color.Blue,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "What is the premium feature?",
                        Value = "While I'm trying to keeping Sanara as open as possible, storage and API calls aren't free\n" +
                            "Therefor some features are now restricted to 'premium' users."
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "How can I apply?",
                        Value = "For now users must be manually whitelisted"
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Radio module",
                        Value = "YouTube API calls are heavily limited and Radio module need a lot of them so letting everyone use the radio would drastically reduce the number of music you would be able to play"
                    }
                }
            }.Build());
        }
    }
}
