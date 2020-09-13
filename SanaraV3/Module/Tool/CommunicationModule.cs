using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Module.Tool
{
    public sealed class CommunicationModule : ModuleBase
    {
        [Command("Quote", RunMode = RunMode.Async)]
        public async Task QuoteAsync(IMessage msg)
        {
            await QuoteInternalAsync(msg);
        }

        private async Task QuoteInternalAsync(IMessage msg)
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                ThumbnailUrl = msg.Author.GetAvatarUrl(),
                Title = msg.Author.ToString(),
                Description = msg.Content.Length == 0 && msg.Embeds.Count == 1 ? msg.Embeds.ToArray()[0].Description : msg.Content,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"The {msg.CreatedAt.ToString("dd/MM/yy at HH:mm:ss")} in #{msg.Channel.Name}"
                }
            }.Build());
        }
    }
}
